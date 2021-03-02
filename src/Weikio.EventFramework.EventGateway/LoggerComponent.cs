using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventGateway
{
    public class LoggerComponent
    {
        private readonly ILogger<LoggerComponent> _logger;

        public LoggerComponent(ILogger<LoggerComponent> logger)
        {
            _logger = logger;
        }

        public Task<CloudEvent> Handle(CloudEvent ev)
        {
            var msg = ev.ToJson();
            _logger.LogDebug(msg);
            
            return Task.FromResult(ev);
        }

        public IDataflowBlock Create()
        {
            var result = new TransformBlock<CloudEvent, CloudEvent>(ev =>
            {
                _logger.LogDebug(ev.ToJson());

                return ev;
            });

            return result;
        }
    }
}