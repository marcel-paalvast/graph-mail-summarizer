using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Models;
public record MailSummary
{
    public required string Summary { get; set; }
    public required Mail Mail { get; set; }
}
