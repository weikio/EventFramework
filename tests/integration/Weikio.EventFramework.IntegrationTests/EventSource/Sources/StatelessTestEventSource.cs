using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.IntegrationTests.EventSource.Sources
{
    [DisplayName("StatelessTestEventSource")]
    public class StatelessTestEventSource
    {
        public Task<NewFileEvent> Run()
        {
            return Task.FromResult(new NewFileEvent("single.txt"));
        }
    }
    
    [DisplayName("EventSourceWithConfigurationType")]
    public class EventSourceWithConfigurationType
    {
        private readonly EventSourceWithConfigurationTypeTheConfigurationType _configuration;

        public EventSourceWithConfigurationType(EventSourceWithConfigurationTypeTheConfigurationType configuration)
        {
            _configuration = configuration;
        }

        public Task<NewFileEvent> Run()
        {
            return Task.FromResult(new NewFileEvent("single.txt"));
        }
    }

    public class EventSourceWithConfigurationTypeTheConfigurationType
    {
        
    }

    public static class EventSourceWithConfigurationTypeExtensions
    {
        public static IEventFrameworkBuilder AddEventSourceWithConfigurationTypeExtensions(this IEventFrameworkBuilder builder,
            Action<EventSourceInstanceOptions> configureInstance = null)
        {
            var services = builder.Services;
            
            services.AddEventSourceWithConfigurationTypeExtensions(configureInstance);

            return builder;
        } 
        
        public static IServiceCollection AddEventSourceWithConfigurationTypeExtensions(this IServiceCollection services,
            Action<EventSourceInstanceOptions> configureInstance = null)
        {
            services.AddEventSource<EventSourceWithConfigurationType>(configureInstance);

            services.Configure<EventSourceConfigurationOptions>(typeof(EventSourceWithConfigurationType).FullName, options =>
            {
                options.ConfigurationType = typeof(EventSourceWithConfigurationTypeTheConfigurationType);
            });
            
            return services;
        } 
    }
}
