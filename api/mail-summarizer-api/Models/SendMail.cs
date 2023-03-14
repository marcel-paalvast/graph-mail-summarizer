using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Models;
public record SendMail
{
    public required string? Subject { get; set; }
    public required string? Recipient { get; set; }
    public required string? Body { get; set; }
}
