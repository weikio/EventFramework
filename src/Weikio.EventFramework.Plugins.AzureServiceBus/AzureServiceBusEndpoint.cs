using System;
using System.Text;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Plugins.AzureServiceBus
{
    public class AzureServiceBusEndpoint
    {
        private readonly AzureServiceBusConfiguration _configuration;
        private readonly ILogger<AzureServiceBusEndpoint> _logger;

        public AzureServiceBusEndpoint(AzureServiceBusConfiguration configuration, ILogger<AzureServiceBusEndpoint> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Send(CloudEvent cloudEvent)
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
        }
    }
}
