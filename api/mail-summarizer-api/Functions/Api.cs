using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using mail_summarizer_api.Models;
using mail_summarizer_api.Services;

namespace mail_summarizer_api.Functions
{
    public class Api
    {
        private readonly ISummarizer _summarizer;
        private readonly IMailService _mailService;

        public Api(ISummarizer summarizer, IMailService mailService)
        {
            _summarizer = summarizer;
            _mailService = mailService;
        }

        [FunctionName("summarize")]
        public async Task<IActionResult> Summarize(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var data = await JsonSerializer.DeserializeAsync<GetSummarization>(req.Body);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(data, new ValidationContext(data), results, true))
            {
                return new BadRequestObjectResult(string.Join(' ', results));
            }

            string responseMessage = $"Retrieving mail from {data.From:yyyy-MM-dd HH:mm:ss} to {data.To:yyyy-MM-dd HH:mm:ss}";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("summarize-text")]
        public async Task<IActionResult> SummarizeText(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var data = await JsonSerializer.DeserializeAsync<string>(req.Body);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(data, new ValidationContext(data), results, true))
            {
                return new BadRequestObjectResult(string.Join(' ', results));
            }

            var summarization = await _summarizer.SummarizeAsync(data);

            return new OkObjectResult(summarization);
        }

        [FunctionName("mails")]
        public async Task<IActionResult> GetMails(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            await _mailService.GetMailAsync(null);

            return new OkObjectResult("OK");
        }
    }
}
