using System;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource.SDK
{
    public static  class EventFrameworkBuilderExtensions
    { 
        public static IEventFrameworkBuilder AddEventSource<TEventSourceType>(this IEventFrameworkBuilder builder, Action<EventSourceInstanceOptions> configureInstance = null)
        {
            var services = builder.Services;

            services.AddSingleton(new EventSourcePlugin()
            {
                EventSourceType = typeof(TEventSourceType), ConfigureInstance = configureInstance
            });
            
            if (configureInstance != null)
            {
                services.AddSingleton(provider =>
                {
                    var options = new EventSourceInstanceOptions();
                    configureInstance(options);
            
                    if (options.EventSourceDefinition == null)
                    {
                        var esProvider = provider.GetRequiredService<IEventSourceProvider>();
                        var def = esProvider.GetByType(typeof(TEventSourceType));

                        options.EventSourceDefinition = def;
                    }
                    
                    return options;
                });
            }

            return builder;
        }
    }
}
