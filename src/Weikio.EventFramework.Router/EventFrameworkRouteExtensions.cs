using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.Extensions.EventAggregator;

namespace Weikio.EventFramework.Router
{
    public static class EventFrameworkRouteExtensions
    {
        public static IEventFrameworkBuilder AddRoute(this IEventFrameworkBuilder builder, string incomingGatewayName, string outgoingGatewayName, Predicate<CloudEvent> filter = null, Func<CloudEvent, IServiceProvider, Task<CloudEvent>> onRouting = null)
        {
            builder.AddHandler<RoutingHandler>(handler =>
            {
                handler.IncomingGatewayName = incomingGatewayName;
                handler.OutgoingGatewayName = outgoingGatewayName;
                handler.Filter = filter;
                handler.OnRouting = onRouting;
            });
            
            return builder;
        }
    }
}
