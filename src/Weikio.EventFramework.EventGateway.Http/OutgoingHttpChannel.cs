using System;
using System.Net.Http;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class OutgoingHttpChannel : IOutgoingChannel
    {
        private readonly Func<HttpClient> _httpClientFactory;
        private readonly string _outgoingEndpoint;
        private readonly Func<HttpClient, Task> _configureClient;

        public OutgoingHttpChannel(Func<HttpClient> httpClientFactory, string name, string outgoingEndpoint, Func<HttpClient, Task> configureClient = null)
        {
            Name = name;
            _httpClientFactory = httpClientFactory;
            _outgoingEndpoint = outgoingEndpoint;
            _configureClient = configureClient;
        }

        public string Name { get; }

        public async Task<bool> Send(object cloudEvents)
        {
            var cloudEvent = (CloudEvent) cloudEvents;
            var client = _httpClientFactory();

            if (_configureClient != null)
            {
                await _configureClient(client);
            }

            var content = new CloudEventContent(cloudEvent,
                ContentMode.Structured,
                new JsonEventFormatter());

            try
            {
                var result = await client.PostAsync(_outgoingEndpoint, content);

                if (result.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw;
            }
        }

        public void Subscribe(IChannel channel)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(IChannel channel)
        {
            throw new NotImplementedException();
        }
    }
}
