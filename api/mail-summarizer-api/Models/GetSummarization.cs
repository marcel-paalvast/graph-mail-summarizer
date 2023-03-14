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
    /// <summary>
    /// Only mails are retrieved that are bigger or equal than the <see cref="From"/> value.
    /// </summary>
    [Required]
    [JsonPropertyName("from")]
    public DateTimeOffset? From { get; set; }

    /// <summary>
    /// Only mails are retrieved that are small than the <see cref="To"/> value.
    /// </summary>
    [Required]
    [JsonPropertyName("to")]
    public DateTimeOffset? To { get; set; }
}
