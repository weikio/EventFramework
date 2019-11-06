namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventGateway
    {
        string Name { get; }
        IIncomingChannel IncomingChannel { get; }
        IOutgoingChannel OutgoingChannel { get; }
        
        bool SupportsIncoming => IncomingChannel != null;
        bool SupportsOutgoing => OutgoingChannel != null;
    }
}
