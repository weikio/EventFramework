using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.ApiFramework;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Apis;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.ApiFramework.SDK;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventGateway.Http.ApiFrameworkIntegration;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.EventFramework.EventGateway.Http
{
    public static class EventFrameworkHttpGatewayExtensions
    {
        public static IEventFrameworkBuilder AddHttpGateways(this IEventFrameworkBuilder builder)
        {
            AddHttpGateways(builder.Services);

            return builder;
        }

        public static IServiceCollection AddHttpGateways(this IServiceCollection services)
        {
            services.AddCloudEventGateway();
            services.AddHealthChecks();
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            if (services.All(x => x.ServiceType != typeof(IEndpointInitializer)))
            {
                // TODO: Collection concurrent problem in Api Framework
                services.TryAddSingleton<IEndpointInitializer, SyncEndpointInitializer>();

                services.AddApiFrameworkCore(options =>
                {
                    options.AutoResolveEndpoints = false;
                    options.EndpointHttpVerbResolver = new CustomHttpVerbResolver();
                });
            }

            return services;
        }

        public static IEventFrameworkBuilder AddHttpGateway(this IEventFrameworkBuilder builder, string name = GatewayName.Default,
            string endpoint = HttpGateway.DefaultEndpoint,
            string outgoingEndpoint = HttpGateway.DefaultOutgoingEndpoint, Action<HttpClient> configureClient = null)
        {

            // builder.Services.AddSingleton<IEndpointConfigurationProvider>(provider =>
            // {
            //     var endpointDefinition = new EndpointDefinition(endpoint, typeof(HttpCloudEventReceiverApi).FullName,
            //         new HttpCloudEventReceiverApiConfiguration() { GatewayName = name }, new EmptyHealthCheck(), string.Empty);
            //
            //     return new CustomEndpointConfigurationProvider(new List<EndpointDefinition>() { endpointDefinition });
            // });

            return builder;
        }

        public static IServiceCollection AddHttpGateway(this IServiceCollection services, string name = GatewayName.Default,
            string endpoint = HttpGateway.DefaultEndpoint,
            string outgoingEndpoint = HttpGateway.DefaultOutgoingEndpoint, Action<HttpClient> configureClient = null, Func<HttpClient> clientFactory = null)
        {

            
            // services.AddSingleton<IEndpointConfigurationProvider>(provider =>
            // {
            //     var endpointDefinition = new EndpointDefinition(endpoint, typeof(HttpCloudEventReceiverApi).FullName,
            //         new HttpCloudEventReceiverApiConfiguration() { GatewayName = name }, new EmptyHealthCheck(), string.Empty);
            //
            //     return new CustomEndpointConfigurationProvider(new List<EndpointDefinition>() { endpointDefinition });
            // });

            return services;
        }

        public static IServiceCollection AddRemoteHttpGateway(this IServiceCollection services, string url,
            Action<RemoteHttpGatewayOptions> configure = null)
        {
            services.AddHttpClient();

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            services.AddSingleton<RemoteHttpGatewayFactory>();

            var opt = new RemoteHttpGatewayOptions { Endpoint = url };
            configure?.Invoke(opt);

            services.Configure<RemoteHttpGatewayOptions>(opt.Name, c =>
            {
                c.Endpoint = url;
                configure?.Invoke(c);
            });

            services.AddTransient(provider =>
            {
                var factory = provider.GetRequiredService<RemoteHttpGatewayFactory>();

                return factory.Create(opt.Name);
            });

            return services;
        }
    }
}
