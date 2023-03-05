using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mail_summarizer_api.Settings;
public record GraphSettings
{
    [JsonPropertyName("TenantId")]
    public string TenantId { get; set; }

    [JsonPropertyName("ClientId")]
    public string ClientId { get; set; }

    [JsonPropertyName("ClientSecret")]
    public string ClientSecret { get; set; }

    [JsonPropertyName("Scope")]
    public string Scope { get; set; }
}
