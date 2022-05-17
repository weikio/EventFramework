using System;
using System.Net.Http;
using System.Threading.Tasks;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Components.Http;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class EventFlowBuilderHttpExtensions
    {
        public static IEventFlowBuilder Http(this IEventFlowBuilder builder, string url, Func<HttpClient, Task> configureClient = null,
            Func<HttpClient> clientFactory = null)
        {
            var httpEndpointBuilder = new HttpEndpoint(new HttpEndpointOptions()
            {
                Endpoint = url, ConfigureClient = configureClient, ClientFactory = clientFactory
            });

            builder.Component(httpEndpointBuilder);

            return builder;
        }
        
        public static CloudEventsChannelBuilder Http(this CloudEventsChannelBuilder builder, string url, Func<HttpClient, Task> configureClient = null,
            Func<HttpClient> clientFactory = null)
        {
            var httpEndpointBuilder = new HttpEndpoint(new HttpEndpointOptions()
            {
                Endpoint = url, ConfigureClient = configureClient, ClientFactory = clientFactory
            });

            builder.Component(httpEndpointBuilder);

            return builder;
        }
    }
}
