using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Models;
public record GetMailOptions
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public int? Top { get; set; }
    public string? Recipient { get; set; }
}
