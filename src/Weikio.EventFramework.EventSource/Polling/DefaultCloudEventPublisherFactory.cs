using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Polling
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
                
                var builder = _serviceProvider.GetRequiredService<ICloudCloudEventPublisherBuilder>();

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

    public interface ICloudCloudEventPublisherBuilder
    {
        CloudEventPublisher Build(IOptions<CloudEventPublisherOptions> options);
    }

    public class DefaultCloudEventPublisherBuilder : ICloudCloudEventPublisherBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICloudEventGatewayManager _gatewayManager;
        private readonly ICloudEventCreator _cloudEventCreator;

        public DefaultCloudEventPublisherBuilder(IServiceProvider serviceProvider, ICloudEventGatewayManager gatewayManager,
            ICloudEventCreator cloudEventCreator)
        {
            _serviceProvider = serviceProvider;
            _gatewayManager = gatewayManager;
            _cloudEventCreator = cloudEventCreator;
        }

        public CloudEventPublisher Build(IOptions<CloudEventPublisherOptions> options)
        {
            var result = new CloudEventPublisher(_gatewayManager, options, _cloudEventCreator, _serviceProvider, _serviceProvider.GetRequiredService<ILogger<CloudEventPublisher>>());

            return result;
        }
    }
}
