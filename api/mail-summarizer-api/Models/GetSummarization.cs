using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mail_summarizer_api.Models;
public record GetSummarization
{
    [Required]
    [JsonPropertyName("from")]
    public DateTime? From { get; set; }

    [Required]
    [JsonPropertyName("to")]
    public DateTime? To { get; set; }
}
