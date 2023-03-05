using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using mail_summarizer_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using mail_summarizer_api.Settings;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Extensions.DependencyInjection;

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

    public async Task GetMailAsync(GetMailOptions options)
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
                c.QueryParameters.Top = options.Top;
                c.QueryParameters.Filter = $"ReceivedDateTime ge {options.From:yyyy-MM-ddTHH:mm:ssZ} and ReceivedDateTime lt {options.To:yyyy-MM-ddTHH:mm:ssZ}";
            });
    }

    public Task SendMailAsync(Mail mail)
    {
        throw new NotImplementedException();
    }
}
