using System;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Configuration
{
    public class EventFrameworkOptions
    {
        public Uri DefaultSource = new Uri("http://localhost/eventframework");
        public string DefaultGatewayName { get; set; } = GatewayName.Default;
    }
}
