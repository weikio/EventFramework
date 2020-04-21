using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Apis;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.ApiFramework.Core.Infrastructure;
using Weikio.AspNetCore.StartupTasks;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.AspNetCore.Gateways;
using Weikio.EventFramework.EventAggregator.AspNetCore;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.Extensions;
using Weikio.EventFramework.Router;

namespace Weikio.EventFramework.AspNetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddEventFramework(this IServiceCollection services, Action<EventFrameworkOptions> setupAction = null)
        {
            var builder = new EventFrameworkBuilder(services);

            builder.AddEventPublisher();
            builder.AddEventCreator();
            builder.AddEventAggregator();
            builder.AddEventGateway();
            
            builder.Services.TryAddSingleton<ICloudEventRouterServiceFactory, CloudEventRouterServiceFactory>();
            builder.Services.TryAddTransient<ICloudEventRouterService, CloudEventRouterService>();
            builder.Services.TryAddSingleton<ICloudEventRouteCollection, CloudEventRouteCollection>();

            builder.Services.AddTransient<HttpGatewayFactory>();
            builder.Services.AddTransient<HttpGatewayInitializer>();
            builder.Services.TryAddSingleton<RouteInitializer>();

            builder.Services.AddStartupTasks();

            // TODO: Collection concurrent problem in Api Framework
            services.AddSingleton<IEndpointInitializer, SyncEndpointInitializer>();

            services.AddApiFrameworkCore(options =>
            {
                options.ApiAddressBase = "";
                options.AutoResolveEndpoints = false;
                options.EndpointHttpVerbResolver = new CustomHttpVerbResolver();
                options.ApiProvider = new TypeApiProvider(typeof(HttpCloudEventReceiverApi));
            });

            var options = new EventFrameworkOptions();
            setupAction?.Invoke(options);
            
            var conf = Options.Create(options);
            builder.Services.AddSingleton<IOptions<EventFrameworkOptions>>(conf);

            builder.Services.AddSingleton<IEndpointConfigurationProvider>(provider =>
            {
                var gatewayCollection = provider.GetRequiredService<ICloudEventGatewayManager>();
                var httpGateways = gatewayCollection.Gateways.OfType<HttpGateway>().ToList();

                var endpoints = new List<EndpointDefinition>();

                foreach (var httpGateway in httpGateways)
                {
                    var endpoint = new EndpointDefinition(httpGateway.Endpoint, typeof(HttpCloudEventReceiverApi).FullName,
                        new HttpCloudEventReceiverApiConfiguration() { GatewayName = httpGateway.Name }, new EmptyHealthCheck(), string.Empty);

                    endpoints.Add(endpoint);
                }

                return new CustomEndpointConfigurationProvider(endpoints);
            });

            builder.AddEventSources();

            return builder;
        }
    }

    public class EventFrameworkOptions
    {
    }

    public class CustomHttpVerbResolver : IEndpointHttpVerbResolver
    {
        public string GetHttpVerb(ActionModel action)
        {
            return "POST";
        }
    }

    public class CustomEndpointConfigurationProvider : IEndpointConfigurationProvider
    {
        private readonly List<EndpointDefinition> _endpointDefinitions;

        public CustomEndpointConfigurationProvider(List<EndpointDefinition> endpointDefinitions)
        {
            _endpointDefinitions = endpointDefinitions;
        }

        public Task<List<EndpointDefinition>> GetEndpointConfiguration()
        {
            return Task.FromResult(_endpointDefinitions);
        }
    }
}
