using System;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventGateway 
    {
        string Name { get; }
        IIncomingChannel IncomingChannel { get; }
        IOutgoingChannel OutgoingChannel { get; }
        
        bool SupportsIncoming => IncomingChannel != null;
        bool SupportsOutgoing => OutgoingChannel != null;
        Task Initialize();
        CloudEventGatewayStatus Status { get; }
    }
}
