using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Services;
public interface ISummarizeService
{
    Task<string> SummarizeAsync(string? message);
    Task<string> SummarizeAsync(IEnumerable<string> messages);
}
