﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.AspNetCore.StartupTasks;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Configuration;
using Weikio.EventFramework.Gateways;
using Weikio.EventFramework.Router;

namespace Weikio.EventFramework.Extensions
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
            
            if (setupAction != null)
            {
                builder.Services.Configure(setupAction);
            }
            
            return builder;
        }
    }
    
    public class CloudEventRouteCollection : List<ICloudEventRoute>, ICloudEventRouteCollection
    {
        public IEnumerable<ICloudEventRoute> Routes => this;
    }
}
