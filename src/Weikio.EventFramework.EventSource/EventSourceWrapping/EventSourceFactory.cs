using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class EventSourceFactory
    {
        public EventSource Create(Func<object, Task<(object, object)>> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return CreateSourceInner(action, pollingFrequency, cronExpression, configure);
        }

        public EventSource Create(Func<object> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return CreateSourceInner(action, pollingFrequency, cronExpression, configure);
        }

        public EventSource Create(object instance, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return CreateSourceInner(null, pollingFrequency, cronExpression, configure, null, instance);
        }

        public EventSource Create<TStateType>(Func<TStateType, (object, TStateType)> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            Func<TStateType, Task<(object, TStateType)>> taskAction = state => Task.FromResult(action(state));

            return CreateSourceInner(taskAction, pollingFrequency, cronExpression, configure);
        }

        public EventSource Create<TStateType>(Func<TStateType, Task<(object, TStateType)>> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return CreateSourceInner(action, pollingFrequency, cronExpression, configure);
        }

        public EventSource Create<TEventSource>(TimeSpan? pollingFrequency = null,
            string cronExpression = null, Action<TEventSource> configure = null)
        {
            return CreateSourceInner(null, pollingFrequency, cronExpression, configure, typeof(TEventSource));
        }

        public EventSource Create(Func<Task<List<object>>> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return CreateSourceInner(action, pollingFrequency, cronExpression, configure);
        }

        public EventSource CreateSourceInner(MulticastDelegate action = null, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Type eventSourceType = null, object eventSourceInstance = null)
        {
            var id = Guid.NewGuid();

            return new EventSource(id, action, pollingFrequency, cronExpression, configure, eventSourceType, eventSourceInstance);

            // if (eventSourceType == null && eventSourceInstance != null)
            // {
            //     eventSourceType = eventSourceInstance.GetType();
            // }
            //
            // var isHostedService = eventSourceType != null && typeof(IHostedService).IsAssignableFrom(eventSourceType);
            //
            // var requiresPollingJob = isHostedService == false;
            //
            //
            //
            // if (requiresPollingJob)
            // {
            //     services.AddSingleton(provider =>
            //     {
            //         if (pollingFrequency == null)
            //         {
            //             var optionsManager = provider.GetService<IOptionsMonitor<PollingOptions>>();
            //             var options = optionsManager.CurrentValue;
            //
            //             pollingFrequency = options.PollingFrequency;
            //         }
            //
            //         var schedule = new PollingSchedule(id, pollingFrequency, cronExpression);
            //
            //         return schedule;
            //     });
            // }
            //
            // if (isHostedService)
            // {
            //     services.AddTransient(typeof(IHostedService), provider =>
            //     {
            //         var inst = ActivatorUtilities.CreateInstance(provider, eventSourceType);
            //
            //         if (configure != null)
            //         {
            //             configure.DynamicInvoke(inst);
            //         }
            //
            //         return inst;
            //     });
            // }
            // else if (eventSourceType != null)
            // {
            //     services.TryAddTransient(eventSourceType);
            //
            //     services.AddTransient(provider =>
            //     {
            //         var logger = provider.GetRequiredService<ILogger<TypeToEventSourceFactory>>();
            //         var factory = new TypeToEventSourceFactory(eventSourceType, id, logger, eventSourceInstance);
            //
            //         return factory;
            //     });
            // }
            // else
            // {
            //     services.AddOptions<JobOptions>(id.ToString())
            //         .Configure<EventSourceActionWrapper>((jobOptions, wrapper) =>
            //         {
            //             var wrapped = wrapper.Wrap(action);
            //             jobOptions.Action = wrapped.Action;
            //             jobOptions.ContainsState = wrapped.ContainsState;
            //         });
            // }
            //
            // return services;
        }
    }
}
