using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource
{
    public class DefaultCloudEventPublisherFactory : ICloudEventPublisherFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DefaultCloudEventPublisherFactory> _logger;

        public DefaultCloudEventPublisherFactory(IServiceProvider serviceProvider,
            ILogger<DefaultCloudEventPublisherFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public CloudEventPublisher CreatePublisher(string name)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                // Get the default settings
                var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<CloudEventPublisherOptions>>().Value;

                // Get the named settings
                var optionsSnapshopt = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CloudEventPublisherFactoryOptions>>();
                var factoryOptions = optionsSnapshopt.Get(name);

                // Run the named settings over the default settings
                Action<CloudEventPublisherOptions> configurator = null;

                if (factoryOptions.ConfigureOptions?.Any() == true)
                {
                    foreach (var configureOption in factoryOptions.ConfigureOptions)
                    {
                        configurator += configureOption;
                    }
                }

                if (configurator != null)
                {
                    configurator(options);
                }
                
                var builder = _serviceProvider.GetRequiredService<ICloudEventPublisherBuilder>();

                var result = builder.Build(new OptionsWrapper<CloudEventPublisherOptions>(options));

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create publisher with name {Name}", name);

                throw;
            }
        }
    }
}
