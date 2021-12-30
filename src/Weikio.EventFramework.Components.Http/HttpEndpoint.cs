using System.Net.Http;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components.Http
{
    public class HttpEndpoint : IComponentBuilder
    {
        public HttpEndpoint(HttpEndpointOptions configuration = null)
        {
            Configuration = configuration ?? new HttpEndpointOptions();
        }

        public HttpEndpointOptions Configuration { get; set; }

        public Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<HttpEndpointComponent>>();
            var httpClientFactory = context.ServiceProvider.GetService<IHttpClientFactory>();

            var result = new HttpEndpointComponent(Configuration, logger, httpClientFactory);

            return Task.FromResult<CloudEventsComponent>(result);
        }
    }
}
