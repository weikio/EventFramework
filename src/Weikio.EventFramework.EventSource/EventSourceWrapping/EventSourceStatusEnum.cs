namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public enum EventSourceStatusEnum
    {
        New,
        Initializing,
        Initialized,
        InitializingFailed,
        Running,
        Stopping,
        Stopped,
        Failed
    }

    public class EventSourceStatus : StatusBase<EventSourceStatusEnum>
    {
        
    }
}
