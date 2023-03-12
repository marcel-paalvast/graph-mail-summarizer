using mail_summarizer_api.Middleware.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mail_summarizer_api.Services.Jwt;
public partial class TokenUserInfoService : IUserInfoService
{
    private IFunctionContextAccessor _functionContext;

    public TokenUserInfoService(IFunctionContextAccessor functionContext)
    {
        _functionContext = functionContext;
    }

    public string? GetMailAddress()
    {
        var principal = _functionContext.FunctionContext?.Features.Get<ClaimsPrincipal>();

        if (principal is null)
        {
            return null;
        }

        var mail = principal.FindFirst(ClaimTypes.Email)?.Value;
        if (mail is null && principal.FindFirst(ClaimTypes.Upn) is Claim upn && IsMailAddress().IsMatch(upn.Value))
        {
            mail = upn.Value;
        }
        if (mail is null && principal.FindFirst("preferred_username") is Claim pu && IsMailAddress().IsMatch(pu.Value))
        {
            mail = pu.Value;
        }

        return mail;
    }

    [GeneratedRegex("^[A-Za-z0-9+_.-]+@[A-Za-z0-9.-]+$")]
    private static partial Regex IsMailAddress();
}
