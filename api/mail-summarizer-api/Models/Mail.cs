using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Models;
public record Mail
{
    public string? Subject { get; set; }
    public string? Sender { get; set; }
    public string? Body { get; set; }
}