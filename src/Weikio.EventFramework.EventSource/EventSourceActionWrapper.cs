using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource
{
    public class EventSourceActionWrapper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICloudEventPublisher _publisher;
        private readonly ILogger<EventSourceActionWrapper> _logger;

        public EventSourceActionWrapper(IServiceProvider serviceProvider, ICloudEventPublisher publisher, ILogger<EventSourceActionWrapper> logger)
        {
            _serviceProvider = serviceProvider;
            _publisher = publisher;
            _logger = logger;
        }

        public Func<object, bool, Task<object>> Create(Func<object, Task<(object CloudEvent, object UpdatedState)>> action)
        {
            var result = new Func<object, bool, Task<object>>(async (state, isFirstRun) =>
            {
                try
                {
                    var cloudEvent = action(state);
                    var res = await cloudEvent;

                    if (res.CloudEvent != null && !isFirstRun)
                    {
                        await _publisher.Publish(res.CloudEvent);
                    }

                    return res.UpdatedState;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to run event source's action");

                    throw;
                }
            });

            return result;
        }
            
        public Func<TStateType, bool, Task<TStateType>> Create<TStateType>(Func<TStateType, Task<(object CloudEvent, TStateType UpdatedState)>> action)
        {
            var result = new Func<TStateType, bool, Task<TStateType>>(async (state, isFirstRun) =>
            {
                try
                {
                    var cloudEvent = action(state);
                    var res = await cloudEvent;

                    if (res.CloudEvent != null && !isFirstRun)
                    {
                        await _publisher.Publish(res.CloudEvent);
                    }

                    return res.UpdatedState;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to run event source's action");

                    throw;
                }
            });

            return result;
        }

            
        public Func<Task> Create(Func<Task<List<object>>> action)
        {
            var result = new Func<Task>(async () =>
            {
                try
                {
                    var cloudEvent = action();
                    var res = await cloudEvent;

                    if (res == null)
                    {
                        return;
                    }

                    await _publisher.Publish(res);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to run event source's action");

                    throw;
                }
            });

            return result;
        }
            
        public Func<TStateType, Task<TStateType>> Create<TStateType>(Func<TStateType, IServiceProvider, Task<(object CloudEvent, TStateType UpdatedState)>> action)
        {
            var result = new Func<TStateType, Task<TStateType>>(async (state) =>
            {
                try
                {
                    var cloudEvent = action(state, _serviceProvider);
                    var res = await cloudEvent;

                    if (res.CloudEvent != null)
                    {
                        await _publisher.Publish(res.CloudEvent);
                    }

                    return res.UpdatedState;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to run event source's action");

                    throw;
                }
            });

            return result;
        }
    }
}
