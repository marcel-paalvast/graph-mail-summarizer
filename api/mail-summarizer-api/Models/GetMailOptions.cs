using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Models;
public record GetMailOptions
{
    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public int? Top { get; set; }

    public string MailFolder { get; set; } = "Inbox";
}
