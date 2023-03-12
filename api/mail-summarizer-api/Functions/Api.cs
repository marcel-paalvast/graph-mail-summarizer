using System;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using mail_summarizer_api.Models;
using mail_summarizer_api.Services;
using System.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using mail_summarizer_api.Middleware.Authorization;
using System.Security.Claims;

namespace mail_summarizer_api.Functions
{
    public class Api
    {
        private readonly ILogger<Api> _logger;
        private readonly ISummarizeService _summarizer;
        private readonly IMailService _mailService;
        private readonly IMailGeneratorService _mailGenerator;
        private readonly IUserInfoService _userInfo;

        public Api(
            ILoggerFactory loggerFactory, 
            ISummarizeService summarizer, 
            IMailService mailService,
            IMailGeneratorService mailGenerator,
            IUserInfoService userInfo)
        {
            _logger = loggerFactory.CreateLogger<Api>();
            _summarizer = summarizer;
            _mailService = mailService;
            _mailGenerator = mailGenerator;
            _userInfo = userInfo;
        }

        [Function(nameof(Summarize))]
        public async Task<HttpResponseData> Summarize(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "summarize")] HttpRequestData req,
            ILogger log)
        {
            var data = await JsonSerializer.DeserializeAsync<GetSummarization>(req.Body);
            var results = new List<ValidationResult>();
            if (data is null || !Validator.TryValidateObject(data, new ValidationContext(data), results, true))
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString(string.Join(' ', results));

                return response;
            }

            string responseMessage = $"Retrieving mail from {data.From:yyyy-MM-dd HH:mm:ss} to {data.To:yyyy-MM-dd HH:mm:ss}";

            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString(responseMessage);

                return response;
            }
        }

        [Function(nameof(SummarizeText))]
        public async Task<HttpResponseData> SummarizeText(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "summarize-text")] HttpRequestData req,
            ILogger log)
        {
            var data = await JsonSerializer.DeserializeAsync<string>(req.Body);
            var results = new List<ValidationResult>();
            if (data is null || !Validator.TryValidateObject(data, new ValidationContext(data), results, true))
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString(string.Join(' ', results));

                return response;
            }

            var summarization = await _summarizer.SummarizeAsync(data);

            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString(summarization);

                return response;
            }
        }

        [Authorize]
        [Function(nameof(GetMails))]
        public async Task<HttpResponseData> GetMails(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "mails")] HttpRequestData req,
            ILogger log)
        {
            var mails = await _mailService.GetMailAsync(new() 
            { 
                Top = 100,
                From = new DateTime(2023,1,1),
                To = new DateTime(2024,1,1),
            });

            var subjects = string.Join(" & ", mails.Select(x => x.Subject));

            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString(subjects);

                return response;
            }
        }

        [Function(nameof(SendMail))]
        public async Task<HttpResponseData> SendMail(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "send")] HttpRequestData req,
            ILogger log)
        {
            await _mailService.SendMailAsync(new SendMail()
            {
                Body = "This is a test mail",
                Recipient = "...",
                Subject = "Test",
            });

            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString("Sent!");

                return response;
            }
        }

        [Function(nameof(GenerateMail))]
        public async Task<HttpResponseData> GenerateMail(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "generate")] HttpRequestData req,
            ILogger log)
        {
            var mail = _mailGenerator.Create(new(), new List<MailSummary>()
            {
                new()
                {
                    Mail = new()
                    {
                        Sender = "Name#1",
                        Subject = "Proposal",
                    },
                    Summary = "Could you give feedback on the following proposal.",
                },
                new()
                {
                    Mail = new()
                    {
                        Sender = "Name#2",
                        Subject = "Meeting",
                    },
                    Summary = "This is a summary of a very long mail.\nWith newline!",
                },
            });

            await _mailService.SendMailAsync(new SendMail()
            {
                Body = mail,
                Recipient = "...",
                Subject = "Test",
            });

            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString("Sent!");

                return response;
            }
        }

        [Authorize]
        [Function(nameof(GetAddress))]
        public async Task<HttpResponseData> GetAddress(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "address")] HttpRequestData req,
            ILogger log)
        {
            var mail = _userInfo.GetMailAddress();

            if (mail is null)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString("Token contains no valid mail address");

                return response;
            }

            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString(mail);

                return response;
            }
        }
    }
}
