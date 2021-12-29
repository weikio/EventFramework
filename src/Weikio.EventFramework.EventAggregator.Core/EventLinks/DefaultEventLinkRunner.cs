using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventAggregator.Core.EventLinks
{
    public class DefaultEventLinkRunner : IEventLinkRunner
    {
        private readonly ILogger<DefaultEventLinkRunner> _logger;
        private Func<MethodInfo, CloudEvent, List<object>> _getArguments;
        private object _handler;
        private MethodInfo _handlerMethod;
        private CloudEventCriteria _criteria;
        private Func<CloudEvent, Task<bool>> _canHandle;
        private MethodInfo _guardMethod;

        public DefaultEventLinkRunner(ILogger<DefaultEventLinkRunner> logger)
        {
            _logger = logger;
        }

        public void Initialize(object handler, MethodInfo handlerMethod, Func<MethodInfo, CloudEvent, List<object>> getArguments,
            CloudEventCriteria criteria, Func<CloudEvent, Task<bool>> canHandle, MethodInfo guardMethod)
        {
            _handler = handler;
            _handlerMethod = handlerMethod;
            _getArguments = getArguments;
            _criteria = criteria;
            _canHandle = canHandle;
            _guardMethod = guardMethod;
        }

        public async Task Handle(CloudEvent cloudEvent, IServiceProvider serviceProvider)
        {
            try
            {
                var arguments = _getArguments(_handlerMethod, cloudEvent);

                var res = (Task) _handlerMethod.Invoke(_handler, arguments.ToArray());
                await res;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to run event");

                throw;
            }
        }

        public async Task<bool> CanHandle(CloudEvent cloudEvent)
        {
            try
            {
                if (!_criteria.CanHandle(cloudEvent))
                {
                    return false;
                }

                if (_canHandle != null)
                {
                    var res = await _canHandle.Invoke(cloudEvent);

                    if (res == false)
                    {
                        return false;
                    }
                }

                if (_guardMethod != null)
                {
                    var arguments = _getArguments(_guardMethod, cloudEvent);
                    
                    var res = (Task<bool>) _guardMethod.Invoke(_handler, arguments.ToArray());
                    await res;

                    return res.Result;
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to guard event");

                return false;
            }
        }
    }
}
