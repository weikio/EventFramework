using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weikio.EventFramework.EventGateway.Gateways.Null;

namespace Weikio.EventFramework.EventGateway
{
    public class DefaultCloudEventChannelManager : List<IChannel>, ICloudEventChannelManager, IDisposable
    {
        public IEnumerable<IChannel> Channels => this;

        public DefaultCloudEventChannelManager()
        {
            var discardChannel = new NullChannel("_discard");
            Add(discardChannel);
        }

        public IChannel Get(string channelName)
        {
            var result = this.FirstOrDefault(x => string.Equals(channelName, x.Name, StringComparison.InvariantCultureIgnoreCase));

            if (result == null)
            {
                if (Count == 1)
                {
                    return this.Single();
                }

                if (Count == 0)
                {
                    throw new NoChannelsConfiguredException();
                }

                throw new UnknownChannelException(channelName);
            }

            return result;
        }

        public void Add(string channelName, IChannel channel)
        {
            Add(channel);
        }

        public Task Update()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            foreach (var channel in Channels)
            {
                if (channel is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }

    public class CloudEventGatewayManager : List<ICloudEventGateway>, ICloudEventGatewayManager
    {
        private readonly ICloudEventGatewayInitializer _initializer;

        public CloudEventGatewayManager(ICloudEventGatewayInitializer initializer)
        {
            _initializer = initializer;
        }

        public IEnumerable<ICloudEventGateway> Gateways => this;

        public ICloudEventGateway Get(string gatewayName)
        {
            var result = this.FirstOrDefault(x => string.Equals(gatewayName, x.Name, StringComparison.InvariantCultureIgnoreCase));

            if (result == null)
            {
                if (Count == 1)
                {
                    return this.Single();
                }

                if (Count == 0)
                {
                    throw new NoGatewaysConfiguredException();
                }

                throw new UnknownGatewayException(gatewayName);
            }

            return result;
        }

        public void Add(string gatewayName, ICloudEventGateway gateway)
        {
            var existing = this.FirstOrDefault(x => string.Equals(gatewayName, x.Name, StringComparison.InvariantCultureIgnoreCase));

            if (existing != null)
            {
                throw new DuplicateGatewayException(gatewayName);
            }

            Add(gateway);
        }

        public async Task Update()
        {
            var removed = new List<ICloudEventGateway>();

            foreach (var cloudEventGateway in Gateways)
            {
                if (cloudEventGateway.Status == CloudEventGatewayStatus.Changed)
                {
                }
                else if (cloudEventGateway.Status == CloudEventGatewayStatus.New)
                {
                    await _initializer.Initialize(cloudEventGateway);
                }
                else if (cloudEventGateway.Status == CloudEventGatewayStatus.Removed)
                {
                    //cloudEventGateway.Dispose();
                    removed.Add(cloudEventGateway);
                }
            }

            foreach (var cloudEventGateway in removed)
            {
                Remove(cloudEventGateway);
            }
        }
    }
}
