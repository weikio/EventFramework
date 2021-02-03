using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Polling
{
    public class DefaultCloudEventPublisherFactory : ICloudEventPublisherFactory
    {
        private readonly IOptionsMonitor<CloudEventPublisherFactoryOptions> _optionsMonitor;
        private readonly ICloudCloudEventPublisherBuilder _builder;
        private readonly ILogger<DefaultCloudEventPublisherFactory> _logger;

        public DefaultCloudEventPublisherFactory(IOptionsMonitor<CloudEventPublisherFactoryOptions> optionsMonitor, ICloudCloudEventPublisherBuilder builder,
            ILogger<DefaultCloudEventPublisherFactory> logger)
        {
            _optionsMonitor = optionsMonitor;
            _builder = builder;
            _logger = logger;
        }

        public CloudEventPublisher CreatePublisher(string name)
        {
            try
            {
                var options = new CloudEventPublisherOptions();
                var factoryOptions = _optionsMonitor.Get(name);

                factoryOptions.ConfigureOptions(options);

                var result = _builder.Build(new OptionsWrapper<CloudEventPublisherOptions>(options));

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
        private readonly IOptionsMonitor<CloudEventCreationOptions> _cloudEventCreationOptionsMonitor;

        public DefaultCloudEventPublisherBuilder(IServiceProvider serviceProvider, ICloudEventGatewayManager gatewayManager,
            ICloudEventCreator cloudEventCreator,
            IOptionsMonitor<CloudEventCreationOptions> cloudEventCreationOptionsMonitor)
        {
            _serviceProvider = serviceProvider;
            _gatewayManager = gatewayManager;
            _cloudEventCreator = cloudEventCreator;
            _cloudEventCreationOptionsMonitor = cloudEventCreationOptionsMonitor;
        }

        public CloudEventPublisher Build(IOptions<CloudEventPublisherOptions> options)
        {
            var result = new CloudEventPublisher(_gatewayManager, options, _cloudEventCreator, _serviceProvider, _cloudEventCreationOptionsMonitor);

            return result;
        }
    }
}
