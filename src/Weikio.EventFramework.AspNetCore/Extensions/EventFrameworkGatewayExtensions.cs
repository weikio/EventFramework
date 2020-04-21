using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.Gateways;

namespace Weikio.EventFramework.AspNetCore.Extensions
{
    public static class EventFrameworkGatewayExtensions
    {
        public static IEventFrameworkBuilder AddGateway(this IEventFrameworkBuilder builder, ICloudEventGateway gateway)
        {
            builder.Services.AddSingleton(provider => gateway);

            return builder;
        }

        public static IEventFrameworkBuilder AddLocal(this IEventFrameworkBuilder builder, string name = GatewayName.Default)
        {
            return builder.AddGateway(new LocalGateway(name));
        }
    }
}
