using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Middleware.Context;

/// <remarks>
/// Source: <see cref="https://gist.github.com/dolphinspired/796d26ebe1237b78ee04a3bff0620ea0"/>.
/// </remarks>
public class FunctionContextMiddleware : IFunctionsWorkerMiddleware
{
    private IFunctionContextAccessor FunctionContextAccessor { get; }

    public FunctionContextMiddleware(IFunctionContextAccessor accessor)
    {
        FunctionContextAccessor = accessor;
    }

    public Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        if (FunctionContextAccessor.FunctionContext != null)
        {
            // This should never happen because the context should be localized to the current Task chain.
            // But if it does happen (perhaps the implementation is bugged), then we need to know immediately so it can be fixed.
            throw new InvalidOperationException($"Unable to initalize {nameof(IFunctionContextAccessor)}: context has already been initialized.");
        }

        FunctionContextAccessor.FunctionContext = context;

        return next(context);
    }
}