using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.Extensions.EventAggregator;

namespace Weikio.EventFramework.Router
{
    public static class EventFrameworkRouteExtensions
    {
        public static IEventFrameworkBuilder AddRoute(this IEventFrameworkBuilder builder, string incomingGatewayName, string outgoingGatewayName, Predicate<CloudEvent> filter = null, Func<CloudEvent, IServiceProvider, Task<CloudEvent>> onRouting = null)
        {
            AddRoute(builder.Services, incomingGatewayName, outgoingGatewayName, filter, onRouting);
            
            return builder;
        }
        
        public static IServiceCollection AddRoute(this IServiceCollection services, string incomingGatewayName, string outgoingGatewayName, Predicate<CloudEvent> filter = null, Func<CloudEvent, IServiceProvider, Task<CloudEvent>> onRouting = null)
        {
            services.AddHandler<CloudEventRoutingHandler>(handler =>
            {
                handler.IncomingGatewayName = incomingGatewayName;
                handler.OutgoingGatewayName = outgoingGatewayName;
                handler.Filter = filter;
                handler.OnRouting = onRouting;
            });
            
            return services;
        }

    }
}
