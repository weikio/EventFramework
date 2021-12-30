using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Weikio.EventFramework.Components.Http
{
    public class HttpEndpointOptions
    {
        public string Endpoint { get; set; }
        public Func<HttpClient, Task> ConfigureClient { get; set; } = client => Task.CompletedTask;
        public Func<HttpClient> ClientFactory { get; set; } 
    }
}
