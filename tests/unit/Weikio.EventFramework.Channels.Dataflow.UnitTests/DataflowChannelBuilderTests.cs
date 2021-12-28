using System;
using System.Collections.Generic;
using System.Linq;
using Weikio.EventFramework.Channels.CloudEvents;
using Xunit;

namespace Weikio.EventFramework.Channels.Dataflow.UnitTests
{
    public class DataflowChannelBuilderTests
    {
        private readonly CloudEventsChannelBuilder _builder = new CloudEventsChannelBuilder();

        [Fact]
        public void CanCreateChannel()
        {
            var channel = _builder.Create();
            Assert.NotNull(channel);
        }
        
        [Fact]
        public void CanCreateChannelWithOptions()
        {
            var options = new CloudEventsChannelOptions() { Name = "test" };
            
            var channel = _builder.Create(options);
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
            var options = new CloudEventsChannelOptions() { Name = null };

            Assert.Throws<ArgumentNullException>(() => _builder.Create(options));
        }
        
        [Fact]
        public void CanCreateChannelWithTags()
        {
            var options = new CloudEventsChannelOptions() { Name = "test", Tags = new List<(string, object)>()
            {
                ("testtag", "value")
            }};
            
            var channel = _builder.Create(options);
            
            Assert.NotEmpty(channel.Tags);
            Assert.Equal("testtag", channel.Tags.Single().Key);
        }
    }
}
