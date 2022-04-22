using NLog.Web;

namespace PlanningApi.Configuration
{
    public static class ConfigureNLog
    {
        public static WebApplicationBuilder AddNLog(this WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            builder.Host.UseNLog();

            return builder;
        }

    }
}
