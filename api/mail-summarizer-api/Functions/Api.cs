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

namespace mail_summarizer_api.Functions
{
    public class Api
    {
        private readonly ILogger<Api> _logger;
        private readonly ISummarizeService _summarizer;
        private readonly IMailService _mailService;

        public Api(ILoggerFactory loggerFactory, ISummarizeService summarizer, IMailService mailService)
        {
            _logger = loggerFactory.CreateLogger<Api>();
            _summarizer = summarizer;
            _mailService = mailService;
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

        [Function(nameof(GetMails))]
        public async Task<HttpResponseData> GetMails(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "mails")] HttpRequestData req,
            ILogger log)
        {
            var mails = await _mailService.GetMailAsync(new() 
            { 
                Top = 100,
                MailFolder = "Inbox",
            });

            var subjects = string.Join(" & ", mails.Select(x => x.Subject));

            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString(subjects);

                return response;
            }
        }
    }
}
