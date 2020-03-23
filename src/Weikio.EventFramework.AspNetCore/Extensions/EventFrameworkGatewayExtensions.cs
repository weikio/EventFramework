using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Gateways;
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

        public static IEventFrameworkBuilder AddHttp(this IEventFrameworkBuilder builder, string name = GatewayName.Default, string endpoint = HttpGateway.DefaultEndpoint, 
            string outgoingEndpoint = HttpGateway.DefaultEndpoint, Action<HttpClient> configureClient = null)
        {
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddTransient(provider =>
            {
                var factory = provider.GetRequiredService<HttpGatewayFactory>();

                return factory.Create(name, endpoint, outgoingEndpoint);
            });

            builder.Services.AddHttpClient(name, client =>
            {
                if (configureClient != null)
                {
                    configureClient(client);
                }
            });

            return builder;
        }
    }
}
