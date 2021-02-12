using System;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource.SDK
{
    public static  class EventFrameworkBuilderExtensions
    { 
        public static IEventFrameworkBuilder AddEventSource<TEventSourceType>(this IEventFrameworkBuilder builder, Action<EventSourceInstanceOptions> configureInstance = null, Type configurationType = null)
        {
            var services = builder.Services;

            services.AddEventSource<TEventSourceType>(configureInstance, configurationType);

            return builder;
        }
        
        public static IServiceCollection AddEventSource<TEventSourceType>(this IServiceCollection services, Action<EventSourceInstanceOptions> configureInstance = null, Type configurationType = null)
        {
            services.AddSingleton(new EventSourcePlugin()
            {
                EventSourceType = typeof(TEventSourceType), ConfigureInstance = configureInstance
            });

            if (configurationType != null)
            {
                services.Configure<EventSourceConfigurationOptions>(typeof(TEventSourceType).FullName, options =>
                {
                    options.ConfigurationType = configurationType;
                });
            }
            
            if (configureInstance != null)
            {
                services.AddSingleton(provider =>
                {
                    var options = new EventSourceInstanceOptions();
                    configureInstance(options);
            
                    if (options.EventSourceDefinition == null)
                    {
                        var esProvider = provider.GetRequiredService<IEventSourceDefinitionProvider>();
                        var def = esProvider.GetByType(typeof(TEventSourceType));

                        options.EventSourceDefinition = def;
                    }
                    
                    return options;
                });
            }

            return services;
        }
    }
}
