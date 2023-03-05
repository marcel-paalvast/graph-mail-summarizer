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

    public GraphMailService(IServiceProvider serviceProvider, IOptions<GraphSettings> options)
    {
        var scopes = new[] { "Mail.Read", "Mail.Send" };
        var authProvider = new OnBehalfOfAuthProvider(options.Value, scopes, serviceProvider);
        _client = new GraphServiceClient(authProvider);
    }

    public async Task GetMailAsync(GetMailOptions options)
    {
        var request = await _client
            .Me
            .Messages
            .GetAsync();
    }

    public Task SendMailAsync(Mail mail)
    {
        throw new NotImplementedException();
    }
}
