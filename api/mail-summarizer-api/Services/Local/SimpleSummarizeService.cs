using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Services.Local;
public class SimpleSummarizeService : ISummarizeService
{
    public Task<string> SummarizeAsync(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return Task.FromResult("");
        }

        var paragraphs = message.Split(new string[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        var builder = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            var sentences = paragraph.Split('.', 2);
            builder.Append(sentences[0]);
            builder.Append(". ");
        }

        return Task.FromResult(builder.ToString());
    }

    public async Task<string> SummarizeAsync(IEnumerable<string> messages)
    {
        var builder = new StringBuilder();
        foreach (var message in messages)
        {
            builder.AppendLine(await SummarizeAsync(message));
        }

        return builder.ToString();
    }
}