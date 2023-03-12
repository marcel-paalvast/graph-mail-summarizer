using mail_summarizer_api.Middleware.Authorization;
using mail_summarizer_api.Models;
using mail_summarizer_api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace mail_summarizer_api.Functions
{
    public class SummarizeMails
    {
        private readonly ILogger<SummarizeMails> _logger;
        private readonly ISummarizeService _summarizer;
        private readonly IMailService _mailService;
        private readonly IMailGeneratorService _mailGenerator;
        private readonly IUserInfoService _userInfo;

        public SummarizeMails(
            ILoggerFactory loggerFactory,
            ISummarizeService summarizer,
            IMailService mailService,
            IMailGeneratorService mailGenerator,
            IUserInfoService userInfo)
        {
            _logger = loggerFactory.CreateLogger<SummarizeMails>();
            _summarizer = summarizer;
            _mailService = mailService;
            _mailGenerator = mailGenerator;
            _userInfo = userInfo;
        }

        [Function(nameof(SummarizeMails))]
        public async Task<string> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context, TokenExtensions<GetMailOptions> input)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(SummarizeMails));
            var mailOptions = input.Value;
            var token = input.Token;

            logger.LogInformation("Obtaining mails.");

            var options = TaskOptions.FromRetryPolicy(new(
                maxNumberOfAttempts: 3,
                firstRetryInterval: TimeSpan.FromSeconds(5)));

            var mails = await context.CallActivityAsync<IList<Mail>>(nameof(GetMailAsync), input, options);

            if (mails.Count == 0)
            {
                return "No mails found";
            }

            var summarizeTasks = new List<Task<MailSummary>>();

            foreach (var mail in mails)
            {
                summarizeTasks.Add(context.CallActivityAsync<MailSummary>(nameof(SummarizeAsync), mail, options));
            }

            try
            {
                await Task.WhenAll(summarizeTasks);
            }
            catch
            {
                if (summarizeTasks.Where(x => x.Status == TaskStatus.RanToCompletion).Count() < 0.5 * summarizeTasks.Count)
                {
                    return "Too many errors occured during summarizaton";
                }
            }

            var summaries = summarizeTasks
                .Where(x => x.Status == TaskStatus.RanToCompletion)
                .Select(x => x.Result)
                .ToList();

            var fullSummary = await context.CallActivityAsync<string>(nameof(SummarizeAllAsync), summaries, options);

            var summary = new MailSummaries()
            {
                FullSummary = fullSummary,
                Summaries = summaries,
                Options = mailOptions,
            };

            var mailBody = await context.CallActivityAsync<string>(nameof(CreateMailTemplate), summary, options);

            var sendMail = new TokenExtensions<SendMail>()
            {
                Token = token,
                Value = new SendMail()
                { 
                    Body = mailBody,
                    Recipient = mailOptions.Recipient,
                    Subject = "Your mail summary",
                },
            };
            await context.CallActivityAsync<string>(nameof(SendMailAsync), sendMail, options);

            return $"Summary of mails has been sent to {mailOptions.Recipient}";
        }

        [Function(nameof(GetMailAsync))]
        public async Task<IList<Mail>> GetMailAsync([ActivityTrigger] TokenExtensions<GetMailOptions> options, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(GetMailAsync));
            executionContext.Features.Set(options.Token);
            logger.LogInformation("Obtaining mails of user");
            return await _mailService.GetMailAsync(options.Value);
        }

        [Function(nameof(SummarizeAsync))]
        public async Task<MailSummary> SummarizeAsync([ActivityTrigger] Mail mail, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(SummarizeAsync));
            logger.LogInformation("Creating summary of {subject}", mail.Subject);

            var summary = await _summarizer.SummarizeAsync(mail.Body);

            return new MailSummary()
            {
                Mail = mail,
                Summary = summary,
            };
        }

        [Function(nameof(SummarizeAllAsync))]
        public async Task<string> SummarizeAllAsync([ActivityTrigger] List<MailSummary> summaries, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(SummarizeAllAsync));
            logger.LogInformation("Creating summary of all mails");

            var summary = await _summarizer.SummarizeAsync(summaries.Select(x => x.Summary));

            return summary;
        }

        [Function(nameof(CreateMailTemplate))]
        public string CreateMailTemplate([ActivityTrigger] MailSummaries summaries, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(CreateMailTemplate));
            logger.LogInformation("Creating summary of all mails");

            var summary = _mailGenerator.Create(summaries);

            return summary;
        }

        [Function(nameof(SendMailAsync))]
        public async Task SendMailAsync([ActivityTrigger] TokenExtensions<SendMail> mail, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(SendMailAsync));
            executionContext.Features.Set(mail.Token);
            logger.LogInformation("Sending mail");

            await _mailService.SendMailAsync(mail.Value);
        }

        //[Function(nameof(SayHello))]
        //public string SayHello([ActivityTrigger] string name, FunctionContext executionContext)
        //{
        //    ILogger logger = executionContext.GetLogger("SayHello");
        //    logger.LogInformation("Saying hello to {name}.", name);
        //    return $"Hello {name}!";
        //}

        [Authorize]
        [Function(nameof(HttpStart))]
        public async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "summarize")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("SummarizeMails_HttpStart");

            var data = await JsonSerializer.DeserializeAsync<GetSummarization>(req.Body);
            var results = new List<ValidationResult>();
            if (data is null || !Validator.TryValidateObject(data, new ValidationContext(data), results, true))
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString(string.Join(' ', results));

                return response;
            }

            var token = executionContext.Features.Get<Token>();

            if (token is null)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString("No token found");

                return response;
            }

            var userMailAddress = _userInfo.GetMailAddress();

            if (userMailAddress is null)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString("Token does not contain a valid mail address");

                return response;
            }

            var mailOptions = new TokenExtensions<GetMailOptions>()
            {
                Token = token,
                Value = new GetMailOptions
                {
                    From = data.From,
                    To = data.To,
                    Top = 100,
                    Recipient = userMailAddress,
                },
            };

            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(SummarizeMails), mailOptions);

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
