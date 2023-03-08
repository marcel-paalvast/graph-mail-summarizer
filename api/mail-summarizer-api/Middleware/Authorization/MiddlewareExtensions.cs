using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Middleware.Authorization;
internal static class MiddlewareExtensions
{
    internal static MethodInfo? GetMethodInfo(this FunctionContext context)
    {
        var entrypoint = context.FunctionDefinition.EntryPoint;
        var lastSeperator = entrypoint.LastIndexOf('.');
        return Type.GetType(entrypoint[..lastSeperator])?.GetMethod(entrypoint[(lastSeperator + 1)..]);
    }
}