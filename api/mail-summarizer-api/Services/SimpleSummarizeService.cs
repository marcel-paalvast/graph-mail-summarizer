using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Services;
public class SimpleSummarizeService : ISummarizeService
{
    public Task<string> SummarizeAsync(string message)
    {
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
}