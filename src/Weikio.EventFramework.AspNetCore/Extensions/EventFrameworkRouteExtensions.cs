using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.AspNetCore.Extensions
{
    public static class EventFrameworkRouteExtensions
    {
        public static IEventFrameworkBuilder AddRoute(this IEventFrameworkBuilder builder, string incomingGatewayName, string outgoingGatewayName, Predicate<ICloudEventContext> filter = null, Func<ICloudEventContext, IServiceProvider, Task<ICloudEventContext>> onRouting = null)
        {
            builder.Services.AddTransient<ICloudEventRoute>(provider =>
            {
                var route = new RouteCloudEventRoute(incomingGatewayName, outgoingGatewayName, provider, filter, onRouting);

                return route;
            });
            
            return builder;
        }

        public static IEventFrameworkBuilder AddRoute(this IEventFrameworkBuilder builder, ICloudEventGateway incomingGateway, ICloudEventGateway outgoingGateway, Func<ICloudEventContext, IServiceProvider, Task<CloudEvent>> onRouting = null)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddRoute(this IEventFrameworkBuilder builder, IIncomingChannel incomingChannel, IOutgoingChannel outgoingChannel, Func<ICloudEventContext, IServiceProvider, Task<CloudEvent>> onRouting = null)
        {
            return builder;
        }
    }
}
