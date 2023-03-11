﻿using Microsoft.Graph;
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

namespace mail_summarizer_api.Services.Graph;
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

        var filter = new MailFilterBuilder();
        filter.AddReceivedFilter();
        if (options.From is DateTime from)
        {
            filter.AddDateFilter(from, DateOperator.GreaterOrEqual);
        }
        if (options.To is DateTime to)
        {
            filter.AddDateFilter(to, DateOperator.LessThan);
        }

        var response = await _client
            .Me
            .Messages
            .GetAsync(c =>
            {
                c.Headers.Add("Prefer", "outlook.body-content-type='text'");
                c.QueryParameters.Select = new[] { "subject", "body", "from", "createdDateTime" };
                c.QueryParameters.Top = options.Top;
                c.QueryParameters.Filter = filter.ToString();
            });

        return response?
            .Value?
            .Where(x => x.Body?.Content is not null)
            .Select(x => new Mail
            {
                CreatedDateTime = x.CreatedDateTime,
                Body = x.Body?.Content,
                Subject = x.Subject,
                Sender = $"{x.From?.EmailAddress?.Name} ({x.From?.EmailAddress?.Address})",
            }) ?? Enumerable.Empty<Mail>();
    }

    public async Task SendMailAsync(SendMail mail)
    {
        var message = new Message
        {
            Subject = mail.Subject,
            Body = new()
            {
                ContentType = BodyType.Html,
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
