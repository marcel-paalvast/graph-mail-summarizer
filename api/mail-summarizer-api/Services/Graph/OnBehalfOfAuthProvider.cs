using mail_summarizer_api.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using mail_summarizer_api.Middleware.Context;
using mail_summarizer_api.Models;

namespace mail_summarizer_api.Services.Graph;
internal class OnBehalfOfAuthProvider : IAuthenticationProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfidentialClientApplication _cca;
    private readonly string[] _scopes;

    public OnBehalfOfAuthProvider(GraphSettings settings, string[] scopes, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _cca = ConfidentialClientApplicationBuilder
            .Create(settings.ClientId)
            .WithTenantId(settings.TenantId)
            .WithClientSecret(settings.ClientSecret)
            .Build();
        _scopes = scopes;
    }

    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        var context = _serviceProvider.GetService<IFunctionContextAccessor>()?.FunctionContext ?? throw new ArgumentException("Could not retrieve function context");
        var token = context.Features.Get<Token>() ?? throw new ArgumentException($"Could not retrieve {nameof(Token)} from context");

        var assertion = new UserAssertion(token.AccessToken);
        var result = await _cca.AcquireTokenOnBehalfOf(_scopes, assertion).ExecuteAsync(cancellationToken);

        request.Headers.Add("Authorization", $"Bearer {result.AccessToken}");
    }
}
