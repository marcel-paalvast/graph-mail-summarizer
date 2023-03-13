using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace mail_summarizer_api.Functions
{
    public class HealthProbe
    {
        private readonly ILogger _logger;

        public HealthProbe(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HealthProbe>();
        }

        [Function("HealthProbe")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "healthprobe")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            return response;
        }
    }
}
