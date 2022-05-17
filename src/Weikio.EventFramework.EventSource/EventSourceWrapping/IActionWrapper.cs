using System;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public interface IActionWrapper
    {
        (Func<Delegate, string, Task<EventPollingResult>> Action, bool ContainsState) Wrap(MethodInfo method);
    }
}
