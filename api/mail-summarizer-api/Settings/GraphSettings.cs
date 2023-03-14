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
    public required string TenantId { get; set; }

    [JsonPropertyName("ClientId")]
    public required string ClientId { get; set; }

    [JsonPropertyName("ClientSecret")]
    public required string ClientSecret { get; set; }

    [JsonPropertyName("Scope")]
    public required string Scope { get; set; }
}
