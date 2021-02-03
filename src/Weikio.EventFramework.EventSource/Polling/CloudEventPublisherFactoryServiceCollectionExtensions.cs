using System;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Polling
{
    public static class CloudEventPublisherFactoryServiceCollectionExtensions
    {
        public static IServiceCollection AddPublisher(this IServiceCollection services, string name, Action<CloudEventPublisherOptions> configurePublisher)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Configure<CloudEventPublisherFactoryOptions>(name, options => options.ConfigureOptions = configurePublisher);

            return services;
        }
    }
}