using System.Threading.Channels;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Mvc;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.AspNetCore.Gateways
{
    public class HttpGateway : ICloudEventGateway
    {
        public HttpGateway(string name, string endpoint)
        {
            Name = name;
            Endpoint = endpoint;
        }

        public string Name { get; }
        public string Endpoint { get; }
        public IIncomingChannel IncomingChannel { get; }
        public IOutgoingChannel OutgoingChannel { get; }
        public bool SupportsIncoming { get; }
        public bool SupportsOutgoing { get; }
    }
    
    public class IncomingHttpChannel :IIncomingChannel
    {
        public string Name { get; }
        public ChannelReader<CloudEvent> Reader { get; }
        public int ReaderCount { get; set; }
    }

    public class HttpCloudEventReceiver
    {
        public async Task ReceiveEvent([FromBody] CloudEvent cloudEvent)
        {
            
        }
    }
}
