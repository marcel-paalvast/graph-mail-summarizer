using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Models;
public class MailSummaries
{
    public required GetMailOptions Options { get; set; }
    public required IList<MailSummary> Summaries { get; set; }
    public required string FullSummary { get; set; }
}
