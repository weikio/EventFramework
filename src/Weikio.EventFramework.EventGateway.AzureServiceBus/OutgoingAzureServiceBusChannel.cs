using System;
using System.Text;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventGateway.AzureServiceBus
{
    public class OutgoingAzureServiceBusChannel : IOutgoingChannel
    {
        private readonly AzureServiceBusOptions _options;
        private readonly ILogger<OutgoingAzureServiceBusChannel> _logger;

        public OutgoingAzureServiceBusChannel(AzureServiceBusOptions options, ILogger<OutgoingAzureServiceBusChannel> logger)
        {
            _options = options;
            _logger = logger;
        }

        public string Name { get; }
        public async Task Send(CloudEvent cloudEvent)
        {
            try
            {
                var json = cloudEvent.ToJson();
                var message = new Message(Encoding.UTF8.GetBytes(json));

                var client = new QueueClient(_options.ConnectionString, _options.QueueName);
                await client.SendAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send cloud event {CloudEvent} to queue {QueueName}", cloudEvent, _options.QueueName);
            }
        }
    }
}
