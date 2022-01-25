using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.SDK;

namespace Weikio.EventFramework.EventSource.AzureServiceBus
{
    public static class EventFrameworkBuilderServiceBusExtensions
    {
        public static IEventFrameworkBuilder AddAzureServiceBusCloudEventSource(this IEventFrameworkBuilder builder, string connectionString, string queue)
        {
            var busConfiguration = new AzureServiceBusCloudEventSourceConfiguration() { ConnectionString = connectionString, QueueName = queue };

            Action<EventSourceInstanceOptions> configureInstance = options =>
            {
                options.Autostart = true;
                options.Id = "asb";
                options.Configuration = busConfiguration;
            };
            
            var services = builder.Services;

            services.AddEventSource<AzureServiceBusCloudEventSource>(configureInstance, typeof(AzureServiceBusCloudEventSourceConfiguration));

            return builder;
        }

        public static IEventFrameworkBuilder AddAzureServiceBusCloudEventSource(this IEventFrameworkBuilder builder,
            Action<EventSourceInstanceOptions> configureInstance = null)
        {
            var services = builder.Services;

            services.AddEventSource<AzureServiceBusCloudEventSource>(configureInstance, typeof(AzureServiceBusCloudEventSourceConfiguration));

            return builder;
        }
    }

    [DisplayName("AzureServiceBusCloudEventSource")]
    public class AzureServiceBusCloudEventSource : BackgroundService
    {
        private readonly ILogger<AzureServiceBusCloudEventSource> _logger;
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private readonly AzureServiceBusCloudEventSourceConfiguration _configuration;

        public AzureServiceBusCloudEventSource(ILogger<AzureServiceBusCloudEventSource> logger, ICloudEventPublisher cloudEventPublisher,
            AzureServiceBusCloudEventSourceConfiguration configuration)
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
