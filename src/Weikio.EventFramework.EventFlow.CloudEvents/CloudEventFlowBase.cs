namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public abstract class CloudEventFlowBase
    {
        public IEventFlowBuilder Flow { get; protected set; }
    }
}
