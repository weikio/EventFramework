using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Router
{
    public class CloudEventRouterService : BackgroundService, ICloudEventRouterService
    {
        private IIncomingChannel _incomingChannel;
        private readonly ICloudEventRouteCollection _cloudEventRouteCollection;
        private readonly ICloudEventAggregator _cloudEventAggregator;
        private readonly ILogger _logger;

        private bool IsInitialized { get; set; }

        public CloudEventRouterService(ILogger<CloudEventRouterService> logger, ICloudEventRouteCollection cloudEventRouteCollection, ICloudEventAggregator cloudEventAggregator)
        {
            _cloudEventRouteCollection = cloudEventRouteCollection;
            _cloudEventAggregator = cloudEventAggregator;
            _logger = logger;
        }

        public void Initialize(IIncomingChannel incomingChannel)
        {
            if (IsInitialized)
            {
                throw new EventRouterServiceAlreadyInitializedException();
            }
            
            _logger.LogDebug("Initializing event route service with {Channel}", incomingChannel);
            _incomingChannel = incomingChannel;
            
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

                    var routes = _cloudEventRouteCollection.Routes;

                    foreach (var route in routes)
                    {
                        var canHandle = await route.CanHandle(cloudEvent);

                        if (!canHandle)
                        {
                            continue;
                        }

                        await route.Handle(cloudEvent);

                        await _cloudEventAggregator.Publish(cloudEvent);
                    }
                }
            }

            _logger.LogInformation("Event router service for {Channel} is stopping.", _incomingChannel);
        }
    }
}
