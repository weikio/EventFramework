using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class DefaultActionWrapper : IActionWrapper
    {
        private readonly ILogger<DefaultActionWrapper> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventSourceInstanceManager _eventSourceInstanceManager;

        public DefaultActionWrapper(ILogger<DefaultActionWrapper> logger, IServiceProvider serviceProvider, IEventSourceInstanceManager eventSourceInstanceManager)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _eventSourceInstanceManager = eventSourceInstanceManager;
        }

        public (Func<Delegate, string, Task<EventPollingResult>> Action, bool ContainsState) Wrap(MethodInfo method)
        {
            var actionParameters = method.GetParameters();

            // Five (and a half) scenarios: 
            // 1.  Task without any return values
            // 2.  Task with value tuple as result where Item1 = new events and Item2 = new state
            // 2.5 Task with EventPollingResult as result
            // 3.  Task with updated state as return value
            // 4.  Task with new event or events as return value
            // 5.  No task 
            var returnType = method.ReturnType;

            var isTask = typeof(Task).IsAssignableFrom(returnType);
            var hasReturnValue = isTask && returnType.IsGenericType;

            if (!hasReturnValue && !isTask)
            {
                hasReturnValue = returnType != typeof(void);
            }

            Func<Task, EventPollingResult> handlingTask = null;

            // Boolean parameter is reserved for the "isFirstRun"-flag
            var hasNonBooleanParameter = actionParameters?.Any(x => typeof(bool).IsAssignableFrom(x.ParameterType) == false);

            var containsState = hasNonBooleanParameter == true;
            ParameterInfo stateType = null;

            if (containsState)
            {
                stateType = actionParameters?.First(x => typeof(bool).IsAssignableFrom(x.ParameterType) == false);
            }

            if (hasReturnValue == false) // scenario 1
            {
                handlingTask = HandleNoResultReturn;
            }
            else
            {
                Type returnValType;

                if (isTask)
                {
                    returnValType = returnType.GenericTypeArguments.First();
                }
                else
                {
                    returnValType = returnType;
                }

                if (typeof(ITuple).IsAssignableFrom(returnValType)) // Scenario 2
                {
                    // We have a method which returns events and the current state

                    handlingTask = HandleValueTuple;
                }
                else if (typeof(EventPollingResult).IsAssignableFrom(returnValType)) // Scenario 2.5
                {
                    handlingTask = HandleEventPollingResult;
                }
                else // Scenario 3 or 4
                {
                    if (containsState)
                    {
                        handlingTask = HandleUpdatedStateResult;
                    }
                    else
                    {
                        handlingTask = HandleNewEventsResult;
                    }
                }
            }

            var result = new Func<Delegate, string, Task<EventPollingResult>>(async (action, id) =>
            {
                try
                {
                    var parameters = new List<object>();
                    var esInstance = _eventSourceInstanceManager.Get(id);

                    var eventSourceInstanceStorageFactory = esInstance.Options.EventSourceInstanceDataStoreFactory(_serviceProvider);
                    var stateStorage = await eventSourceInstanceStorageFactory.GetStorage(id);
                    var state = await stateStorage.LoadState();

                    var isFirstRun = await stateStorage.HasRun() == false;

                    // TODO: Check for parameter declaration errors.
                    if (containsState && stateType != null)
                    {
                        dynamic deserializedState = state != null ? JsonConvert.DeserializeObject(state, stateType.ParameterType) : null;

                        if (actionParameters.Length == 1)
                        {
                            if (typeof(bool).IsAssignableFrom(actionParameters.Single().ParameterType))
                            {
                                parameters.Add(isFirstRun);
                            }
                            else
                            {
                                parameters.Add(deserializedState);
                            }
                        }
                        else if (actionParameters.Count() == 2)
                        {
                            parameters.Add(deserializedState);
                            parameters.Add(isFirstRun);
                        }
                    }
                    else
                    {
                        if (actionParameters.Length == 1)
                        {
                            if (typeof(bool).IsAssignableFrom(actionParameters.Single().ParameterType))
                            {
                                parameters.Add(isFirstRun);
                            }
                        }
                    }

                    Task cloudEvent = null;

                    _logger.LogDebug("Running the scheduled event source with {Id}. Is first run: {IsFirstRun}, current state: {CurrentState}", id, isFirstRun,
                        state);

                    if (!isTask)
                    {
                        cloudEvent = Task.FromResult(action.DynamicInvoke(parameters.ToArray()));
                    }
                    else
                    {
                        cloudEvent = (Task)action.DynamicInvoke(parameters.ToArray());
                    }

                    if (cloudEvent == null)
                    {
                        throw new Exception("The event sources should always return a task");
                    }

                    await cloudEvent;
                    var eventPollingResult = handlingTask(cloudEvent);

                    // TODO: Convert this to stream(?) based system but for now just pass strings around
                    var updatedStateString = eventPollingResult.NewState != null ? JsonConvert.SerializeObject(eventPollingResult.NewState, Formatting.Indented) : "";
                    await stateStorage.Save(updatedStateString);

                    eventPollingResult.IsFirstRun = isFirstRun;
                    return eventPollingResult;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to run event source's action");

                    throw;
                }
            });

            return (result, containsState);
        }

        private EventPollingResult HandleNoResultReturn(Task pollingResult)
        {
            return new EventPollingResult() { NewEvents = new List<object>(), NewState = default(int) };
        }

        private EventPollingResult HandleValueTuple(Task pollingResult)
        {
            ITuple actionResult = ((dynamic)pollingResult).Result;

            var eventsType = actionResult[0]?.GetType();
            var upatedState = actionResult[1];

            if (eventsType == null)
            {
                // This means that event source completed but no new events were products
                return new EventPollingResult() { NewEvents = null, NewState = upatedState };
            }

            var newCloudEvents = new List<object>();

            if (typeof(IEnumerable).IsAssignableFrom(eventsType))
            {
                var newCloudEventsAsEnumerable = (IEnumerable<object>)actionResult[0];
                newCloudEvents.AddRange(newCloudEventsAsEnumerable);
            }
            else
            {
                newCloudEvents.Add(actionResult[0]);
            }

            return new EventPollingResult() { NewEvents = newCloudEvents, NewState = upatedState };
        }

        private EventPollingResult HandleUpdatedStateResult(Task pollingResult)
        {
            object newState = ((dynamic)pollingResult).Result;

            return new EventPollingResult() { NewEvents = new List<object>(), NewState = newState };
        }

        private EventPollingResult HandleNewEventsResult(Task pollingResult)
        {
            object newEvents = ((dynamic)pollingResult).Result;
            var newCloudEvents = new List<object>();

            if (newEvents is IEnumerable)
            {
                newCloudEvents.AddRange((IEnumerable<object>)newEvents);
            }
            else
            {
                newCloudEvents.Add(newEvents);
            }

            return new EventPollingResult() { NewEvents = newCloudEvents, NewState = null };
        }

        private EventPollingResult HandleEventPollingResult(Task pollingResult)
        {
            EventPollingResult result = ((dynamic)pollingResult).Result;

            return result;
        }
    }
}
