using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Crm.Link.UUID.Configuration
{
    public static class UUIDConfig
    {
        public static IServiceCollection UseUUID(this IServiceCollection service)
        {
            service.AddHttpClient("UuidMasterApi", httpClient => {
                httpClient.BaseAddress = new Uri("http://uuidmasterapi-api-1/api");
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            service.AddTransient<IUUIDGateAway, UUIDGateAway>();

            return service;
        }
    }
}
