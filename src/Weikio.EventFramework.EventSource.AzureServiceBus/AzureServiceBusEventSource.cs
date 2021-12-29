using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.AzureServiceBus
{
    public class AzureServiceBusEventSource : BackgroundService
    {
        private readonly ILogger<AzureServiceBusEventSource> _logger;
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private readonly AzureServiceBusConfiguration _configuration;

        public AzureServiceBusEventSource(ILogger<AzureServiceBusEventSource> logger, ICloudEventPublisher cloudEventPublisher,
            AzureServiceBusConfiguration configuration)
        {
            _logger = logger;
            _cloudEventPublisher = cloudEventPublisher;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var receiver = new MessageReceiver(_configuration.ConnectionString, _configuration.QueueName, ReceiveMode.PeekLock);

            // This is the host application's cancellation token
            stoppingToken.Register(() => receiver.CloseAsync().Wait(stoppingToken));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var message = await receiver.ReceiveAsync();

                    if (message == null)
                    {
                        continue;
                    }

                    var notificationBodyJson = Encoding.UTF8.GetString(message.Body);
                    var cloudEvent = notificationBodyJson.ToCloudEvent();

                    await _cloudEventPublisher.Publish(cloudEvent);

                    try
                    {
                        await receiver.CompleteAsync(message.SystemProperties.LockToken);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Handled message, but failed to complete it. Lock timed out? Allow to fail");
                    }
                }
                catch (ServiceBusException e)
                {
                    if (!e.IsTransient)
                    {
                        _logger.LogError(e, "Error processing the message. Try again and eventually deadletter");
                    }
                    else
                    {
                        _logger.LogError(e, "Service bus error, try again shortly");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to handle incoming message");
                }
            }

            _logger.LogInformation("Exiting message handling loop");
        }
    }
}
