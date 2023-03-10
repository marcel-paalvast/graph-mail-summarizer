using mail_summarizer_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mail_summarizer_api.Services;
public partial class BasicMailGeneratorService : IMailGeneratorService
{
    const string Template = """
        <!DOCTYPE html>
        <html lang="en" xmlns="http://www.w3.org/1999/xhtml" xmlns:o="urn:schemas-microsoft-com:office:office">

          <head>
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
            <title>Mail Summaries</title>
            <style>
              body {
                box-sizing: border-box;
                margin: 0;
                font-family: Arial, Helvetica, sans-serif;
              }

              h1,
              h2,
              h3,
              h4,
              h5,
              h6 {
                font-family: Georgia, Arial, Helvetica, sans-serif;
              }

              td > .mail:last-child {
                border: 0;
              }

              .mail {
                padding: 20px;
                border-bottom: 1px solid #d3d3d3;
              }

              .sender {
                color: #bb3030;
              }

              .summary {
                padding-bottom: 10px;
              }

              .timestamp {
                font-family: Consolas, monaco, monospace;
                color: #d3d3d3;
                font-size: 12px;
              }

              .title {
                color: #0d3259;
              }

              .main {
                border-collapse: collapse;
                border-spacing: 0;
              }

              td {
                border-collapse: initial;
                padding: 0;
              }

            </style>
          </head>

          <body>
            <table role="presentation" style="width:100%; background:#ffffff; border-collapse: collapse; border-spacing: 0">
              <tr>
                <td align="center" style="padding:0;">
                  <table class="main" style="width: 700px">
                    <tr>
                      <td style="padding: 40px; background-color: #7cc4e1;">
                        <h2 class="title">Your personal mail summary!</h2>
                        <span>From ${from} till ${to} you've received ${count} mails. Here's a short summary of all the mails followed by a summary of each seperate mail!</span>
                        <br><br>
                        <span>${complete}</span>
                      </td>
                    </tr>
                    <tr>
                    ${summaries}
                    </tr>
                  </table>
                </td>
              </tr>
            </table>

          </body>

        </html>        
        """;

    const string SummaryTemplate = """
        <table class="mail" style="width: 660px">
        <tr>
            <td>
            <table style="width: 100%">
                <tr>
                <td>
                    <h3 class="subject">${subject}</h3>
                </td>
                <td align="right">
                    <h4 class="sender">${sender}</h4>
                </td>
                </tr>
            </table>
            </td>
        </tr>
        <tr>
            <td class="summary">${summary}</td>
        </tr>
        <tr>
            <td class="timestamp" align="right">
            ${timestamp}
            </td>
        </tr>
        </table>
        """;

    public string Create(GetMailOptions settings, IEnumerable<MailSummary> mailSummaries)
    {
        var summaries = string.Join(null, mailSummaries.Select(x =>
        {
            var keywords = new Dictionary<string, (bool encode, string value)>
            {
                ["subject"] = (true, x.Mail.Subject ?? "<No subject>"),
                ["summary"] = (true, x.Summary),
                ["sender"] =(true, x.Mail.Sender ?? "Unknown"),
                ["timestamp"] = (true, x.Mail.CreatedDateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "<No date>"),
            };
            return Replace(SummaryTemplate, keywords);
        }));

        var keywords = new Dictionary<string, (bool encode, string value)>
        {
            ["from"] = (true, settings.From?.ToString("yyyy-MM-dd HH:mm:ss") ?? "<No date>"),
            ["to"] = (true, settings.To?.ToString("yyyy-MM-dd HH:mm:ss") ?? "<No date>"),
            ["summaries"] = (false, summaries),
        };

        return Replace(Template, keywords);
    }

    public static string Replace(string input, Dictionary<string, (bool encode, string value)> keywords)
    {
        return RegexKeywords().Replace(input, m =>
        {
            if (keywords.TryGetValue(m.Groups[1].Value, out var keyword))
            {
                return keyword.encode ? HtmlEncoder.Default.Encode(keyword.value).Replace("&#xA;", "<br>") : keyword.value;
            }
            return "";
        });
    }

    [GeneratedRegex(@"\${(.{1,32}?)}")]
    private static partial Regex RegexKeywords();
}
