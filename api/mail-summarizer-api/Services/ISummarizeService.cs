using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Services;
/// <summary>
/// Summarizes text.
/// </summary>
public interface ISummarizeService
{
    /// <summary>
    /// Summarizes a message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<string> SummarizeAsync(string? message);

    /// <summary>
    /// Summarizes multiple messages into a single message.
    /// </summary>
    /// <param name="messages"></param>
    /// <returns></returns>
    Task<string> SummarizeAsync(IEnumerable<string> messages);
}
