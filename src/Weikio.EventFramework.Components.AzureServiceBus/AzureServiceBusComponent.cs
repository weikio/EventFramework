using System;
using System.Text;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components.AzureServiceBus
{
    public class AzureServiceBusComponent : CloudEventsComponent
    {
        private readonly AzureServiceBusConfiguration _configuration;
        private readonly ILogger<AzureServiceBusComponent> _logger;

        public AzureServiceBusComponent(AzureServiceBusConfiguration configuration, ILogger<AzureServiceBusComponent> logger)
        {
            _configuration = configuration;
            _logger = logger;

            Func = Send;
        }

        private async Task<CloudEvent> Send(CloudEvent cloudEvent)
        {
            try
            {
                var json = cloudEvent.ToJson();
                var message = new Message(Encoding.UTF8.GetBytes(json));

                var client = new QueueClient(_configuration.ConnectionString, _configuration.QueueName);
                await client.SendAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send cloud event {CloudEvent} to queue {QueueName}", cloudEvent, _configuration.QueueName);
            }

            return cloudEvent;
        } 
    }
}