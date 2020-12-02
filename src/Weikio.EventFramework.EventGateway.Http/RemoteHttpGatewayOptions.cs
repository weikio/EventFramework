using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class RemoteHttpGatewayOptions
    {
        public string Endpoint { get; set; }
        public string Name { get; set; } = "RemoteHttpGateway";
        public Func<HttpClient, Task> ConfigureClient { get; set; } = client => Task.CompletedTask;

        public Func<HttpClient> ClientFactory { get; set; } 

    }
}
