using System;
using Xunit;

namespace Weikio.EventFramework.Channels.Dataflow.UnitTests
{
    public class DataflowChannelBuilderTests
    {
        private IChannelBuilder _builder = new DataflowChannelBuilder();

        [Fact]
        public void CanCreateChannel()
        {
            var channel = _builder.Create();
            Assert.NotNull(channel);
        }
        
        [Fact]
        public void ChannelNameDefaultsToDefaultName()
        {
            var channel = _builder.Create();
            
            Assert.Equal(ChannelName.Default, channel.Name);
        }
        
        [Fact]
        public void CanCreateChannelWithName()
        {
            var channel = _builder.Create("channelName");
            
            Assert.Equal("channelName", channel.Name);
        }

        [Fact]
        public void CanNotCreateChannelWithNullName()
        {
            Assert.Throws<ArgumentNullException>(() => _builder.Create(null));
        }
    }
}

// [Fact]
// public async Task DataflowsWork()
// {
//     var provider = Init(services =>
//     {
//         services.AddLocal();
//     });
//
//     var gwManager = provider.GetRequiredService<ICloudEventChannelManager>();
//     var gw = new CloudEventGateway("test", null, new DataflowChannel(gwManager, "test", "local"));
//
//     var c = gw.OutgoingChannel;
//     await c.Send(CloudEventCreator.Create(new CustomerCreatedEvent()));
//     await Task.Delay(TimeSpan.FromSeconds(5));
// }
