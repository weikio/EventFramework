﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventAggregator.Core;
using Weikio.EventFramework.Router;

namespace Weikio.EventFramework.EventGateway
{
    public class CloudEventRouterService : BackgroundService, ICloudEventRouterService
    {
        private IIncomingChannel _incomingChannel;
        private readonly ICloudEventAggregator _cloudEventAggregator;
        private readonly ILogger _logger;
        private ICloudEventGateway _gateway;

        private bool IsInitialized { get; set; }

        public CloudEventRouterService(ILogger<CloudEventRouterService> logger, ICloudEventAggregator cloudEventAggregator)
        {
            _cloudEventAggregator = cloudEventAggregator;
            _logger = logger;
        }

        public void Initialize(IIncomingChannel incomingChannel, ICloudEventGateway gateway)
        {
            if (IsInitialized)
            {
                throw new EventRouterServiceAlreadyInitializedException();
            }
            
            _logger.LogDebug("Initializing event route service with {Channel}", incomingChannel);
            _incomingChannel = incomingChannel;
            _gateway = gateway;
            
            IsInitialized = true;
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            await ExecuteAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(
            CancellationToken cancellationToken)
        {
            if (!IsInitialized)
            {
                throw new EventRouterServiceNotInitializedException();
            }
            
            _logger.LogInformation("Event router service for {Channel} is starting.", _incomingChannel);

            var reader = _incomingChannel.Reader;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                while (await reader.WaitToReadAsync(cancellationToken))
                {
                    if (!reader.TryRead(out var cloudEvent))
                    {
                        continue;
                    }

                    var extension = new EventFrameworkCloudEventExtension(_gateway.Name, _incomingChannel.Name);
                    extension.Attach(cloudEvent);

                    try
                    {
                        await _cloudEventAggregator.Publish(cloudEvent);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed with {CloudEvent} in {Gateway} {Channel}", cloudEvent, _gateway.Name, _incomingChannel.Name);
                    }
                }
            }

            _logger.LogInformation("Event router service for {Channel} is stopping.", _incomingChannel);
        }
    }
}