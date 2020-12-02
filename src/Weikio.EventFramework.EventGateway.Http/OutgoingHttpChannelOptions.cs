using System;
using System.Net.Http;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class OutgoingHttpChannelOptions
    {
        public Action<HttpClient, CloudEvent> BeforeRequest { get; set; } = (client, cloudEvent) => { };
    }
}