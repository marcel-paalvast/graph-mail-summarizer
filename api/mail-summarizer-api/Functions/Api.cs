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

namespace mail_summarizer_api.Functions
{
    public static class Api
    {
        [FunctionName("summarize")]
        public static async Task<IActionResult> Run(
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
    }
}
