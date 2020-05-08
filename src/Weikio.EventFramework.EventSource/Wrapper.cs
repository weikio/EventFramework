using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventSource
{
    public class Wrapper
    {
        public Wrapper()
        {
        }

        public (Func<Delegate, object, bool, Task<EventPollingResult>> Action, bool ContainsState) Wrap(MethodInfo method)
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

            var containsState = actionParameters?.Any() == true;

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

            var result = new Func<Delegate, object, bool, Task<EventPollingResult>>(async (action, state, isFirstRun) =>
            {
                try
                {
                    var parameters = new List<object>();

                    // TODO: Check for parameter declaration errors.
                    if (containsState)
                    {
                        if (actionParameters.Count() == 1)
                        {
                            parameters.Add(state);
                        }
                        else if (actionParameters.Count() == 2)
                        {
                            parameters.Add(state);
                            parameters.Add(isFirstRun);
                        }
                    }

                    Task cloudEvent = null;

                    if (!isTask)
                    {
                        cloudEvent = Task.FromResult(action.DynamicInvoke(parameters.ToArray()));
                    }
                    else
                    {
                        cloudEvent = (Task) action.DynamicInvoke(parameters.ToArray());
                    }

                    if (cloudEvent == null)
                    {
                        throw new Exception("The event sources should always return a task");
                    }

                    await cloudEvent;
                    var eventPollingResult = handlingTask(cloudEvent);

                    return eventPollingResult;
                }
                catch (Exception e)
                {
                    // _logger.LogError(e, "Failed to run event source's action");

                    throw;
                }
            });

            return (result, containsState);
        }

        private EventPollingResult HandleNoResultReturn(Task pollingResult)
        {
            return new EventPollingResult(){NewEvents = new List<object>(), NewState = default(int)};
        }

        private EventPollingResult HandleValueTuple(Task pollingResult)
        {
            ITuple actionResult = ((dynamic) pollingResult).Result;

            var eventsType = actionResult[0]?.GetType();

            if (eventsType == null)
            {
                throw new ArgumentNullException();
            }

            var newCloudEvents = new List<object>();

            if (typeof(IEnumerable).IsAssignableFrom(eventsType))
            {
                var newCloudEventsAsEnumerable = (IEnumerable<object>) actionResult[0];
                newCloudEvents.AddRange(newCloudEventsAsEnumerable);
            }
            else
            {
                newCloudEvents.Add(actionResult[0]);
            }

            var upatedState = actionResult[1];

            return new EventPollingResult() { NewEvents = newCloudEvents, NewState = upatedState };
        }

        private EventPollingResult HandleUpdatedStateResult(Task pollingResult)
        {
            object newState = ((dynamic) pollingResult).Result;

            return new EventPollingResult(){NewEvents = new List<object>(), NewState = newState};
        }

        private EventPollingResult HandleNewEventsResult(Task pollingResult)
        {
            object newEvents = ((dynamic) pollingResult).Result;
            var newCloudEvents = new List<object>();

            if (newEvents is IEnumerable)
            {
                newCloudEvents.AddRange((IEnumerable<object>) newEvents);
            }
            else
            {
                newCloudEvents.Add(newEvents);
            }

            return new EventPollingResult(){NewEvents = newCloudEvents, NewState = null};
        }
        
        private EventPollingResult HandleEventPollingResult(Task pollingResult)
        {
            EventPollingResult result = ((dynamic) pollingResult).Result;

            return result;
        }
    }
}
