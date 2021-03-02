using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weikio.EventFramework.Channels.Dataflow
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
}
