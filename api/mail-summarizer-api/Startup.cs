using mail_summarizer_api.Services;
using mail_summarizer_api.Settings;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(mail_summarizer_api.Startup))]
namespace mail_summarizer_api;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddOptions<GraphSettings>().Configure<IConfiguration>((settings, config) =>
        {
            config.GetSection("Graph").Bind(settings);
        });

        builder.Services.AddSingleton<ISummarizer, SimpleSummarizer>();
        builder.Services.AddSingleton<IMailService, GraphMailService>();
    }
}
