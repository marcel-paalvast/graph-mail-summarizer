using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using mail_summarizer_api.Models;
using Microsoft.Extensions.Options;
using mail_summarizer_api.Settings;
using Microsoft.Graph.Models;

namespace mail_summarizer_api.Services;
public class GraphMailService : IMailService
{
    private readonly GraphServiceClient _client;

    public GraphMailService(HttpClient httpClient, IServiceProvider serviceProvider, IOptions<GraphSettings> options)
    {
        var scopes = new[] { "Mail.Read", "Mail.Send" };
        var authProvider = new OnBehalfOfAuthProvider(options.Value, scopes, serviceProvider);
        _client = new GraphServiceClient(httpClient, authProvider);
    }

    public async Task<IEnumerable<Mail>> GetMailAsync(GetMailOptions options)
    {
        options ??= new();
        options.From ??= DateTime.MinValue;
        options.To ??= DateTime.MaxValue;

        var response = await _client
            .Me
            .MailFolders[options.MailFolder]
            .Messages
            .GetAsync(c =>
            {
                c.Headers.Add("Prefer", "outlook.body-content-type='text'");
                c.QueryParameters.Select = new[] { "subject", "body", "sender" };
                c.QueryParameters.Top = options.Top;
                c.QueryParameters.Filter = $"ReceivedDateTime ge {options.From:yyyy-MM-ddTHH:mm:ssZ} and ReceivedDateTime lt {options.To:yyyy-MM-ddTHH:mm:ssZ}";
            });

        return response?
            .Value?
            .Select(x => new Mail
            {
                Body = x.Body?.Content,
                Subject = x.Subject,
                Sender = $"{x.Sender?.EmailAddress?.Name} ({x.Sender?.EmailAddress?.Address})",
            }) ?? Enumerable.Empty<Mail>();
    }

    public async Task SendMailAsync(SendMail mail)
    {
        var message = new Message
        {
            Subject = mail.Subject,
            Body = new()
            {
                ContentType = BodyType.Text,
                Content = mail.Body,
            },
            ToRecipients = new()
            {
                new Recipient { EmailAddress = new EmailAddress { Address = mail.Recipient }},
            }
        };

        await _client
            .Me
            .SendMail
            .PostAsync(new() { Message = message });
    }
}
