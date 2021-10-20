using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Weikio.EventFramework.Samples.EventSource
{
    [DisplayName("TestEventSource")]
    public class TestEventSource
    {

        public Task<(CountEvent NewEvent, int NewState)> IncrementCount(int currentState)
        {
            var newState = currentState + 1;

            return Task.FromResult((new CountEvent(newState), newState));
        }
    }
}
