namespace Weikio.EventFramework.EventSource.Abstractions
{
    public enum EventSourceStatusEnum
    {
        New,
        Initializing,
        Initialized,
        InitializingFailed,
        Starting,
        Started,
        Stopping,
        Stopped,
        Removed,
        Failed
    }
}
