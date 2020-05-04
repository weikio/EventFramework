using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventGateway.Gateways.Local;

namespace Weikio.EventFramework.AspNetCore.Extensions
{
    public static class LocalEventFrameworkGatewayExtensions
    {
        public static IEventFrameworkBuilder AddGateway(this IEventFrameworkBuilder builder, ICloudEventGateway gateway)
        {
            builder.Services.AddGateway(gateway);

            return builder;
        }
        
        public static IServiceCollection AddGateway(this IServiceCollection services, ICloudEventGateway gateway)
        {
            services.AddSingleton(provider => gateway);

            return services;
        }

        public static IEventFrameworkBuilder AddLocal(this IEventFrameworkBuilder builder, string name = GatewayName.Default)
        {
            builder.Services.AddLocal(name);

            return builder;
        }

        public static IServiceCollection AddLocal(this IServiceCollection services, string name = GatewayName.Default)
        {
            return services.AddGateway(new LocalGateway(name));
        }
    }
}
