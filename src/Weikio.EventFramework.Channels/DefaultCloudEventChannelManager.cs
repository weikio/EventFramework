using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Weikio.EventFramework.Channels
{
    public class DefaultChannelManager : List<IChannel>, IChannelManager, IDisposable
    {
        private DefaultChannelOptions _defaultChannelOptions;
        public IEnumerable<IChannel> Channels => this;

        public DefaultChannelManager(IOptions<DefaultChannelOptions> defaultChannelOptions)
        {
            var discardChannel = new NullChannel("_discard");
            Add(discardChannel);
            _defaultChannelOptions = defaultChannelOptions.Value;
        }

        public IChannel GetDefaultChannel()
        {
            var result = this.FirstOrDefault(x => string.Equals(_defaultChannelOptions.DefaultChannelName, x.Name, StringComparison.InvariantCultureIgnoreCase));

            if (result != null)
            {
                return result;
            }

            if (Count == 1)
            {
                return this.Single();
            }

            throw new NoDefaultChannelFoundException();
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

    public class DefaultChannelOptions
    {
        public string DefaultChannelName { get; set; } = ChannelName.Default;
    }
}
