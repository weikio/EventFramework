using System.Threading.Channels;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Mvc;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Gateways;

namespace Weikio.EventFramework.AspNetCore.Gateways
{
    public class HttpGateway : ICloudEventGateway
    {
        public HttpGateway(string name, string endpoint)
        {
            Name = name;
            Endpoint = endpoint;
            
            var channel = Channel.CreateUnbounded<CloudEvent>();

            IncomingChannel = new IncomingHttpChannel(channel);
            OutgoingChannel = null;
        }

        public string Name { get; }
        public string Endpoint { get; }
        public IIncomingChannel IncomingChannel { get; }
        public IOutgoingChannel OutgoingChannel { get; }
        public bool SupportsIncoming { get; }
        public bool SupportsOutgoing { get; }
    }
    
    public class IncomingHttpChannel : IIncomingChannel
    {
        public IncomingHttpChannel(Channel<CloudEvent> channel)
        {
            Writer = channel.Writer;
            Reader = channel.Reader;
        }

        public string Name { get; }
        public ChannelWriter<CloudEvent> Writer { get; }
        public ChannelReader<CloudEvent> Reader { get; }
        public int ReaderCount { get; set; }
    }

    public class HttpCloudEventReceiverApi
    {
        private readonly ICloudEventGatewayCollection _cloudEventGatewayCollection;

        public HttpCloudEventReceiverApi(ICloudEventGatewayCollection cloudEventGatewayCollection)
        {
            _cloudEventGatewayCollection = cloudEventGatewayCollection;
        }

        public HttpCloudEventReceiverApiConfiguration Configuration { get; set; }

        public async Task ReceiveEvent(CloudEvent cloudEvent)
        {
            // Assert policy

            var attr = cloudEvent.GetAttributes();
            
            var gateway = _cloudEventGatewayCollection.Get(Configuration.GatewayName);
            var channel = gateway.IncomingChannel;

            await channel.Writer.WriteAsync(cloudEvent);
        }
    }

    public class HttpCloudEventReceiverApiConfiguration
    {
        public string GatewayName { get; set; }
        public string InputChannelName { get; set; }
        public string PolicyName { get; set; }
    }
}
