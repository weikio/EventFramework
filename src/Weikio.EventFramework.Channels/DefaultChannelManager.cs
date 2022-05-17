using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels
{
    public class DefaultChannelManager : Dictionary<string, IChannel>, IChannelManager, IDisposable, IAsyncDisposable
    {
        private readonly DefaultChannelOptions _defaultChannelOptions;
        public IEnumerable<IChannel> Channels => this.Select(x => x.Value);

        public DefaultChannelManager(IOptions<DefaultChannelOptions> defaultChannelOptions) : base(StringComparer.InvariantCultureIgnoreCase)
        {
            _defaultChannelOptions = defaultChannelOptions.Value;
        }

        public void Add(IChannel channel)
        {
            _defaultChannelOptions.InitAction?.Invoke(channel);
            
            Add(channel.Name, channel);
        }

        public IChannel GetDefaultChannel()
        {
            if (ContainsKey(_defaultChannelOptions.DefaultChannelName))
            {
                return this[_defaultChannelOptions.DefaultChannelName];
            }

            throw new NoDefaultChannelFoundException();
        }

        public void Remove(IChannel channel)
        {
            Remove(channel.Name);
        }

        public IChannel Get(string channelName)
        {
            if (ContainsKey(channelName))
            {
                return this[channelName];
            }
            
            if (Count == 0)
            {
                throw new NoChannelsConfiguredException();
            }

            throw new UnknownChannelException(channelName);
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

        public async ValueTask DisposeAsync()
        {
            foreach (var channel in Channels)
            {
                if (channel is IAsyncDisposable disposable)
                {
                    await disposable.DisposeAsync();
                }
            }
        }
    }
}
