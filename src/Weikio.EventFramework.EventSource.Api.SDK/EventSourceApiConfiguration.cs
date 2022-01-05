using System;

namespace Weikio.EventFramework.EventSource.Api.SDK
{
    public class EventSourceApiConfiguration
    {
        public Type ApiType { get; set; }
        public object ApiConfiguration { get; set; }
    }
}