using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Middleware.Authorization;
/// <summary>
/// Require authorization for this action.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
class AuthorizeAttribute : Attribute
{
    /// <summary>
    /// Require one of the folowing roles. Comma seperated.
    /// </summary>
    /// <remarks>
    /// Multiple <see cref="AuthorizeAttribute"/> can be stacked to require multiple roles. Whitespaces are not allowed.
    /// </remarks>
    public string? Roles { get; set; } = "";
}

