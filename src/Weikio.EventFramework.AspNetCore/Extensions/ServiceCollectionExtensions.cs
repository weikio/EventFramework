using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Apis;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.ApiFramework.Core.Infrastructure;
using Weikio.AspNetCore.StartupTasks;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Gateways;
using Weikio.EventFramework.Configuration;
using Weikio.EventFramework.EventAggregator;
using Weikio.EventFramework.Extensions;
using Weikio.EventFramework.Publisher;
using Weikio.EventFramework.Router;

namespace Weikio.EventFramework.AspNetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddEventFramework(this IServiceCollection services, Action<EventFrameworkOptions> setupAction = null)
        {
            var builder = new EventFrameworkBuilder(services);

            builder.Services.TryAddSingleton<ICloudEventPublisher, CloudEventPublisher>();
            builder.Services.TryAddSingleton<ICloudEventGatewayCollection, CloudEventGatewayCollection>();
            builder.Services.TryAddSingleton<ICloudEventAggregator, CloudEventAggregator>();
            builder.Services.TryAddSingleton<ICloudEventRouterServiceFactory, CloudEventRouterServiceFactory>();
            builder.Services.TryAddTransient<ICloudEventRouterService, CloudEventRouterService>();
            builder.Services.TryAddSingleton<ICloudEventRouteCollection, CloudEventRouteCollection>();
            builder.Services.AddStartupTasks();
            
            services.AddHttpContextAccessor();

            services.AddApiFrameworkCore(options =>
            {
                options.ApiAddressBase = "";
                options.AutoResolveEndpoints = false;
                options.EndpointHttpVerbResolver = new CustomHttpVerbResolver();
                options.ApiProvider = new TypeApiProvider(typeof(HttpCloudEventReceiverApi));
            });
            
            if (setupAction != null)
            {
                builder.Services.Configure(setupAction);
            }

            builder.Services.Configure<MvcOptions>(options =>
            {
                options.InputFormatters.Insert(0, new CloudEventJsonInputFormatter());
            });

            builder.Services.AddSingleton<IEndpointConfigurationProvider>(provider =>
            {
                var gatewayCollection = provider.GetRequiredService<ICloudEventGatewayCollection>();
                var httpGateways = gatewayCollection.Gateways.OfType<HttpGateway>().ToList();

                var endpoints = new List<EndpointDefinition>();
                foreach (var httpGateway in httpGateways)
                {
                    var endpoint = new EndpointDefinition(httpGateway.Endpoint, typeof(HttpCloudEventReceiverApi).FullName, new HttpCloudEventReceiverApiConfiguration()
                    {
                        GatewayName = httpGateway.Name
                    }, new EmptyHealthCheck());
                    
                    endpoints.Add(endpoint);
                }
                
                return new CustomEndpointConfigurationProvider(endpoints);
            });
            
            return builder;
        }
    }

    public class CustomHttpVerbResolver :IEndpointHttpVerbResolver
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
