using mail_summarizer_api.Middleware.Context;
using mail_summarizer_api.Services;
using mail_summarizer_api.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(s =>
    {
        s.UseMiddleware<FunctionContextMiddleware>();
    })
    .ConfigureServices(s =>
    {
        s.AddSingleton<IFunctionContextAccessor, FunctionContextAccessor>();
        s.AddHttpClient();
        s.AddOptions<GraphSettings>().Configure<IConfiguration>((settings, config) =>
        {
            config.GetSection("Graph").Bind(settings);
        });
        s.AddSingleton<IMailService, GraphMailService>();

        s.AddOptions<OpenAiSettings>().Configure<IConfiguration>((settings, config) =>
        {
            config.GetSection("OpenAi").Bind(settings);
        });
        s.AddSingleton<ISummarizeService, OpenAiSummarizeService>();
    })
    .Build();

host.Run();