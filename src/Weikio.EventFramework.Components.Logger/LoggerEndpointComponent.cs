using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components.Logger
{
    public class LoggerEndpointComponent : CloudEventsComponent
    {
        private readonly ILogger<LoggerEndpointComponent> _logger;
        private readonly LoggerEndpointOptions _configuration;

        public LoggerEndpointComponent(LoggerEndpointOptions configuration, ILogger<LoggerEndpointComponent> logger)
        {
            _configuration = configuration;
            _logger = logger;

            Func = Log;
        }

        public Task<CloudEvent> Log(CloudEvent cloudEvent)
        {
            _logger.Log(_configuration.LogLevel, cloudEvent.ToJson());

            return Task.FromResult(cloudEvent);
        }
    }
}
