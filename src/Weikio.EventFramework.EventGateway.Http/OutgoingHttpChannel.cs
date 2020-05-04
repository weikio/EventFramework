using System;
using System.Net.Http;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class OutgoingHttpChannel : IOutgoingChannel
    {
        private readonly Func<HttpClient> _httpClientFactory;
        private readonly string _outgoingEndpoint;

        public OutgoingHttpChannel(Func<HttpClient> httpClientFactory, string name, string outgoingEndpoint)
        {
            Name = name;
            _httpClientFactory = httpClientFactory;
            _outgoingEndpoint = outgoingEndpoint;
        }

        public string Name { get; }

        public async Task Send(CloudEvent cloudEvent)
        {
            var client = _httpClientFactory();

            var content = new CloudEventContent(cloudEvent,
                ContentMode.Structured,
                new JsonEventFormatter());

            try
            {
                var result = await client.PostAsync(_outgoingEndpoint, content);

                if (result.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw;
            }
        }
    }
}
