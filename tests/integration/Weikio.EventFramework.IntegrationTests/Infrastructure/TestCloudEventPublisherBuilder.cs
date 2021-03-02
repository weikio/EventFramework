﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.IntegrationTests.Infrastructure
{
    public class TestCloudEventPublisherBuilder : ICloudEventPublisherBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public TestCloudEventPublisherBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public CloudEventPublisher Build(IOptions<CloudEventPublisherOptions> options)
        {
            var gatewayManager = _serviceProvider.GetRequiredService<ICloudEventGatewayManager>();
            var cloudEventCreator = _serviceProvider.GetRequiredService<ICloudEventCreator>();
            var channelManager = _serviceProvider.GetRequiredService<ICloudEventChannelManager>();

            var result = new MyTestCloudEventPublisher(gatewayManager, options, cloudEventCreator, _serviceProvider, channelManager);

            return result;
        }
    }
}
