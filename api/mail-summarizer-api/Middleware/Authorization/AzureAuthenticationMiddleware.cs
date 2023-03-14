using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using mail_summarizer_api.Settings;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using mail_summarizer_api.Models;

namespace mail_summarizer_api.Middleware.Authorization;
/// <summary>
/// Checks for an Azure jwt token in the header and validates it using Azure AD.
/// </summary>
/// <remarks>
/// Created for the use of v2.0 tokens.
/// </remarks>
public class AzureAuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    readonly JwtSecurityTokenHandler TokenHandler = new();
    readonly GraphSettings Settings;
    readonly ConfigurationManager<OpenIdConnectConfiguration> ConfigManager;
    private readonly Dictionary<string, List<AuthorizeAttribute>> FunctionAuthorizations;

    public AzureAuthenticationMiddleware(IOptions<GraphSettings> options)
    {
        Settings = options.Value;
        var openIdUri = $"https://login.microsoftonline.com/{Settings.TenantId}/v2.0/.well-known/openid-configuration";
        ConfigManager = new ConfigurationManager<OpenIdConnectConfiguration>(openIdUri, new OpenIdConnectConfigurationRetriever());

        //obtains all functions exposed in the app and collect all AuthorizeAttributes
        FunctionAuthorizations = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.IsClass && x.IsPublic)
            .SelectMany(x => x
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Select(y => new { Method = y, Attribute = y.GetCustomAttribute<FunctionAttribute>(inherit: false) })
                )
            .Where(x => x.Attribute is not null)
            .ToDictionary(x => x.Attribute!.Name, x => x.Method.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList());
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var authorizeAttributes = FunctionAuthorizations[context.FunctionDefinition.Name];

        if (authorizeAttributes.Count > 0)
        {
            var request = await context.GetHttpRequestDataAsync() ?? throw new Exception("Expected http request data");
            var token = GetAccessToken(request);

            if (token is null)
            {
                var response = request.CreateResponse();
                response.StatusCode = HttpStatusCode.Unauthorized;
                context.GetInvocationResult().Value = response;
                return;
            }

            var config = await ConfigManager.GetConfigurationAsync();

            var validationParameters = new TokenValidationParameters()
            {
                NameClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier",
                IssuerSigningKeys = config.SigningKeys,
                TryAllIssuerSigningKeys = false,
                ValidAlgorithms = config.IdTokenSigningAlgValuesSupported,
                ValidAudience = Settings.ClientId,
                ValidIssuer = config.Issuer,
                ValidateIssuer = Settings.TenantId != "common", //not entirely valid; multi-tenant is not per se allowed for all tenants
            };

            try
            {
                var principal = TokenHandler.ValidateToken(token, validationParameters, out var securityToken);
                context.Features.Set(principal);
                context.Features.Set(new Token()
                {
                    AccessToken = token,
                });

                var roles = principal.FindAll(x => x.Type == ClaimTypes.Role).Select(c => c.Value).ToHashSet();

                if (!ValidateRoles(authorizeAttributes, roles))
                {
                    var response = request.CreateResponse();
                    response.StatusCode = HttpStatusCode.Forbidden;
                    context.GetInvocationResult().Value = response;
                    return;
                }
            }
            catch (SecurityTokenValidationException ex)
            {
                var log = context.GetLogger<AzureAuthenticationMiddleware>();
                log.LogWarning(ex, $"{nameof(AzureAuthenticationMiddleware)} validation error");

                var response = request.CreateResponse();
                response.StatusCode = HttpStatusCode.Forbidden;
                context.GetInvocationResult().Value = response;
                return;
            }
        }

        //execute function
        await next(context);
    }

    private static bool ValidateRoles(List<AuthorizeAttribute> authorizeAttributes, HashSet<string> roles)
    {
        return authorizeAttributes.All(attr => string.IsNullOrWhiteSpace(attr.Roles) || (attr.Roles?.Split(',').Any(role => roles.Contains(role)) ?? true));
    }

    private static string? GetAccessToken(Microsoft.Azure.Functions.Worker.Http.HttpRequestData request)
    {
        if (!request.Headers.TryGetValues("Authorization", out var headers))
        {
            return null;
        }
        var split = headers.First().Split(' ');
        return split.Length == 2 && split[0] == "Bearer" ? split[1] : null;
    }
}