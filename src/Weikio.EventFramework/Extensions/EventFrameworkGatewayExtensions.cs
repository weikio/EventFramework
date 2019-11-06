using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Gateways;

namespace Weikio.EventFramework.Extensions
{
    public static class EventFrameworkGatewayExtensions
    {
        public static IEventFrameworkBuilder AddGateway(this IEventFrameworkBuilder builder, ICloudEventGateway gateway)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddGateway(this IEventFrameworkBuilder builder, string name, ICloudEventGateway gateway)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddLocal(this IEventFrameworkBuilder builder)
        {
            builder.Services.AddSingleton<ICloudEventGateway>(provider =>
            {
                var gateway = new LocalGateway();

                return gateway;
            });
            
            return builder;
        }
    }
}
