using System;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventGateway
{
    public class CloudEventGateway : ICloudEventGateway
    {
        private readonly Func<ICloudEventGateway, Task> _initializer;
        private CancellationToken _cancellationToken;

        public CloudEventGateway(string name, Func<ICloudEventGateway, Task> initializer)
        {
            Name = name;
            _initializer = initializer;
        }
        
        public CloudEventGateway(string name, IIncomingChannel incomingChannel, IOutgoingChannel outgoingChannel)
        {
            Name = name;
            IncomingChannel = incomingChannel;
            OutgoingChannel = outgoingChannel;

            Status = CloudEventGatewayStatus.New;
        }

        public string Name { get; }
        public IIncomingChannel IncomingChannel { get; }
        public IOutgoingChannel OutgoingChannel { get; }
        public bool SupportsIncoming => true;
        public bool SupportsOutgoing => true;
        
        public async Task Initialize()
        {
            if (_initializer != null)
            {
                await _initializer(this);
            }
            
            var cancelToken = new CancellationTokenSource();

            _cancellationToken = new CancellationToken();
            
            Status = CloudEventGatewayStatus.Ready;
        }

        public CloudEventGatewayStatus Status { get; protected set; }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}
