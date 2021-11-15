using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions.DependencyInjection;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public static class IEventFrameworkBuilderExtensions
    {
        public static IEventFrameworkBuilder AddIntegrationFlows(this IEventFrameworkBuilder builder)
        {
            var services = builder.Services;
            services.AddIntegrationFlows();

            return builder;
        }

        public static IServiceCollection AddIntegrationFlows(this IServiceCollection services)
        {
            services.AddHostedService<IntegrationFlowStartupService>();
            services.TryAddSingleton<ICloudEventsIntegrationFlowManager, DefaultCloudEventsIntegrationFlowManager>();

            return services;
        }

        public static IEventFrameworkBuilder AddIntegrationFlow(this IEventFrameworkBuilder builder, IntegrationFlowBuilder flowBuilder)
        {
            var services = builder.Services;
            services.AddIntegrationFlow(flowBuilder);

            return builder;
        }

        public static IEventFrameworkBuilder AddIntegrationFlow(this IEventFrameworkBuilder builder, CloudEventsIntegrationFlow flow)
        {
            var services = builder.Services;
            services.AddIntegrationFlow(flow);

            return builder;
        }

        public static IServiceCollection AddIntegrationFlow(this IServiceCollection services, IntegrationFlowBuilder flowBuilder)
        {
            services.AddIntegrationFlows();
            services.AddSingleton(flowBuilder);

            return services;
        }

        public static IEventFrameworkBuilder AddIntegrationFlow<TFlowType>(this IEventFrameworkBuilder builder, MulticastDelegate configure = null,
            object configuration = null)
        {
            var services = builder.Services;

            services.AddIntegrationFlow(typeof(TFlowType), configure, configuration);

            return builder;
        }
        
        public static IEventFrameworkBuilder AddIntegrationFlow<TFlowType>(this IEventFrameworkBuilder builder, object configuration)
        {
            var services = builder.Services;

            services.AddIntegrationFlow(typeof(TFlowType), null, configuration);

            return builder;
        }

        public static IEventFrameworkBuilder AddIntegrationFlow(this IEventFrameworkBuilder builder, Type flowType, MulticastDelegate configure = null,
            object configuration = null)
        {
            var services = builder.Services;

            services.AddIntegrationFlow(flowType, configure, configuration);

            return builder;
        }
        
        public static IEventFrameworkBuilder AddIntegrationFlow(this IEventFrameworkBuilder builder, Type flowType, object configuration)
        {
            var services = builder.Services;

            services.AddIntegrationFlow(flowType, null, configuration);

            return builder;
        }

        public static IServiceCollection AddIntegrationFlow<TFlowType>(this IServiceCollection services, Action<TFlowType> configure = null,
            object configuration = null)
        {
            services.AddIntegrationFlow(typeof(TFlowType), configure, configuration);

            return services;
        }
        
        public static IServiceCollection AddIntegrationFlow<TFlowType>(this IServiceCollection services, object configuration)
        {
            services.AddIntegrationFlow(typeof(TFlowType), null, configuration);

            return services;
        }

        public static IServiceCollection AddIntegrationFlow(this IServiceCollection services, Type flowType, MulticastDelegate configure = null,
            object configuration = null)
        {
            services.AddIntegrationFlows();

            services.AddSingleton<CloudEventsIntegrationFlowFactory>(provider =>
            {
                var result = new CloudEventsIntegrationFlowFactory(serviceProvider =>
                {
                    CloudEventsIntegrationFlowBase instance;

                    if (configuration != null)
                    {
                        instance = (CloudEventsIntegrationFlowBase)ActivatorUtilities.CreateInstance(provider, flowType, new object[] { configuration });
                    }
                    else
                    {
                        instance = (CloudEventsIntegrationFlowBase)ActivatorUtilities.CreateInstance(provider, flowType);
                    }

                    configure?.DynamicInvoke(instance);

                    return instance.Flow.Build(serviceProvider);
                });

                return result;
            });

            return services;
        }

        public static IServiceCollection AddIntegrationFlow(this IServiceCollection services, CloudEventsIntegrationFlow flow)
        {
            services.AddIntegrationFlows();
            services.AddSingleton(flow);

            return services;
        }
    }

    public class IntegrationFlowStartupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IntegrationFlowStartupService> _logger;
        private readonly ICloudEventsIntegrationFlowManager _flowManager;
        private readonly List<CloudEventsIntegrationFlowFactory> _flowFactories;
        private readonly List<CloudEventsIntegrationFlow> _flows;
        private readonly List<IntegrationFlowBuilder> _flowBuilders;

        public IntegrationFlowStartupService(IServiceProvider serviceProvider, ILogger<IntegrationFlowStartupService> logger,
            IEnumerable<CloudEventsIntegrationFlow> flows, IEnumerable<IntegrationFlowBuilder> flowBuilders, ICloudEventsIntegrationFlowManager flowManager,
            IEnumerable<CloudEventsIntegrationFlowFactory> flowFactories)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _flowManager = flowManager;
            _flowFactories = flowFactories.ToList();
            _flows = flows.ToList();
            _flowBuilders = flowBuilders.ToList();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Executing integration flows. Flow count: {FlowCount}, Flow builder count: {FlowBuilderCount}, Flow factory count: {FlowFactoryCount}",
                _flows.Count,
                _flowBuilders.Count, _flowFactories.Count);

            foreach (var flow in _flows)
            {
                try
                {
                    _logger.LogDebug("Executing flow {FlowId}", flow.Id);

                    await _flowManager.Execute(flow);

                    _logger.LogDebug("Flow {FlowId} executed successfully", flow.Id);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to execute flow {FlowId}", flow.Id);
                }
            }

            foreach (var flowBuilder in _flowBuilders)
            {
                try
                {
                    _logger.LogDebug("Creating flow from builder {BuilderId}", flowBuilder.Id);

                    var flow = await flowBuilder.Build(_serviceProvider);

                    _logger.LogDebug("Flow {FlowId} built successfully", flow.Id);

                    _logger.LogDebug("Executing flow {FlowId}", flow.Id);

                    await _flowManager.Execute(flow);

                    _logger.LogDebug("Flow {FlowId} executed successfully", flow.Id);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to build and execute flow {BuilderId}", flowBuilder.Id);
                }
            }

            foreach (var flowFactory in _flowFactories)
            {
                try
                {
                    _logger.LogDebug("Creating flow from factory");

                    var flow = await flowFactory.Create(_serviceProvider);

                    _logger.LogDebug("Flow {FlowId} built successfully", flow.Id);

                    _logger.LogDebug("Executing flow {FlowId}", flow.Id);

                    await _flowManager.Execute(flow);

                    _logger.LogDebug("Flow {FlowId} executed successfully", flow.Id);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to create and execute flow from factory");
                }
            }
        }
    }
}
