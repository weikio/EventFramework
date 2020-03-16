using System.Net.Http;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.AspNetCore.Gateways
{
    public class OutgoingHttpChannel : IOutgoingChannel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _outgoingEndpoint;

        public OutgoingHttpChannel(IHttpClientFactory httpClientFactory, string name, string outgoingEndpoint)
        {
            Name = name;
            _httpClientFactory = httpClientFactory;
            _outgoingEndpoint = outgoingEndpoint;
        }

        public string Name { get; }
        
        public async Task Send(CloudEvent cloudEvent)
        {
            var client = _httpClientFactory.CreateClient(Name);
            
            var content = new CloudEventContent( cloudEvent,
                ContentMode.Structured,
                new JsonEventFormatter());

            await client.PostAsync(_outgoingEndpoint, content);
        }
    }
}
