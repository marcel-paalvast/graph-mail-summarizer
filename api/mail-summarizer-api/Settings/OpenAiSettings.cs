using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mail_summarizer_api.Settings;
public record OpenAiSettings
{
    [JsonPropertyName("ApiKey")]
    public string ApiKey { get; set; }
}

