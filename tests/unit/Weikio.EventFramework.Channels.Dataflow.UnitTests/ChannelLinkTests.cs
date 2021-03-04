using System;
using System.Threading.Tasks;
using Weikio.EventFramework.Channels.Dataflow.CloudEvents;
using Weikio.EventFramework.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.Channels.Dataflow.UnitTests
{
    public class ChannelLinkTests : TestBase
    {
        public ChannelLinkTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ChannelCanSubscribeToAnotherChannel()
        {
            var firstCounter = 0;
            var secondCounter = 0;

            var firstOptions = new CloudEventsDataflowChannelOptions()
            {
                Name = "first",
                Endpoint = ev =>
                {
                    firstCounter += 10;
                },
                LoggerFactory = _loggerFactory
            };

            var secondOptions = new CloudEventsDataflowChannelOptions()
            {
                Name = "second",
                Endpoint = ev =>
                {
                    secondCounter += 3;
                },
                LoggerFactory = _loggerFactory
            };

            var count = 10;
            var objects = CreateObjects(count);

            var channel1 = new CloudEventsDataflowChannel(firstOptions);
            var channel2 = new CloudEventsDataflowChannel(secondOptions);

            channel1.Subscribe(channel2);

            await channel1.Send(objects);

            await channel1.DisposeAsync();
            await channel2.DisposeAsync();

            Assert.Equal(count * 10, firstCounter);
            Assert.Equal(count * 3, secondCounter);
        }
        
        [Fact]
        public async Task ChannelIsByDefaultPubSub()
        {
            var secondReceivedMessage = false;
            var thirdReceivedMessage = false;

            var firstOptions = new CloudEventsDataflowChannelOptions()
            {
                Name = "first",
                LoggerFactory = _loggerFactory
            };

            var secondOptions = new CloudEventsDataflowChannelOptions()
            {
                Name = "second",
                Endpoint = ev =>
                {
                    secondReceivedMessage = true;
                },
                LoggerFactory = _loggerFactory
            };

            var thirdOptions = new CloudEventsDataflowChannelOptions()
            {
                Name = "third",
                Endpoint = ev =>
                {
                    thirdReceivedMessage = true;
                },
                LoggerFactory = _loggerFactory
            };

            
            var channel1 = new CloudEventsDataflowChannel(firstOptions);
            var channel2 = new CloudEventsDataflowChannel(secondOptions);
            var channel3 = new CloudEventsDataflowChannel(thirdOptions);

            channel1.Subscribe(channel2);
            channel1.Subscribe(channel3);

            await channel1.Send(new InvoiceCreated());

            await channel1.DisposeAsync();
            await channel2.DisposeAsync();
            await channel3.DisposeAsync();

            Assert.True(secondReceivedMessage);
            Assert.True(thirdReceivedMessage);
        }
        
        [Fact]
        public async Task CanCreateNonPubSubChannel()
        {
            var secondReceivedMessage = false;
            var thirdReceivedMessage = false;

            var firstOptions = new CloudEventsDataflowChannelOptions()
            {
                Name = "first",
                LoggerFactory = _loggerFactory,
                IsPubSub = false
            };

            var secondOptions = new CloudEventsDataflowChannelOptions()
            {
                Name = "second",
                Endpoint = ev =>
                {
                    secondReceivedMessage = true;
                },
                LoggerFactory = _loggerFactory
            };

            var thirdOptions = new CloudEventsDataflowChannelOptions()
            {
                Name = "third",
                Endpoint = ev =>
                {
                    thirdReceivedMessage = true;
                },
                LoggerFactory = _loggerFactory
            };

            
            var channel1 = new CloudEventsDataflowChannel(firstOptions);
            var channel2 = new CloudEventsDataflowChannel(secondOptions);
            var channel3 = new CloudEventsDataflowChannel(thirdOptions);

            channel1.Subscribe(channel2);
            channel1.Subscribe(channel3);

            await channel1.Send(new InvoiceCreated());

            await channel1.DisposeAsync();
            await channel2.DisposeAsync();
            await channel3.DisposeAsync();

            Assert.True(secondReceivedMessage);
            Assert.False(thirdReceivedMessage);
        }

        [Fact]
        public async Task ChannelCanUnsubscribeFromAnotherChannel()
        {
            var firstCounter = 0;
            var secondCounter = 0;

            var firstOptions = new CloudEventsDataflowChannelOptions()
            {
                Name = "first",
                Endpoint = ev =>
                {
                    firstCounter += 10;
                },
                LoggerFactory = _loggerFactory
            };

            var secondOptions = new CloudEventsDataflowChannelOptions()
            {
                Name = "second",
                Endpoint = ev =>
                {
                    secondCounter += 3;
                },
                LoggerFactory = _loggerFactory
            };

            var count = 10;
            var objects = CreateObjects(count);

            var channel1 = new CloudEventsDataflowChannel(firstOptions);
            var channel2 = new CloudEventsDataflowChannel(secondOptions);

            channel1.Subscribe(channel2);
            await channel1.Send(objects);

            await ContinueWhen(() => firstCounter == count * 10, timeout: TimeSpan.FromSeconds(180));
            await ContinueWhen(() => secondCounter == count * 3, timeout: TimeSpan.FromSeconds(180));

            channel1.Unsubscribe(channel2);

            await channel1.Send(objects);

            await channel1.DisposeAsync();
            await channel2.DisposeAsync();

            Assert.True(count * 10 * 2 == firstCounter, $"First counter should contain {count * 2 * 10}. Instead contains {firstCounter}");
            Assert.True(count * 3 * 1 == secondCounter, $"Second counter should contain {count * 1 * 3}. Instead contains {secondCounter}");
        }
        
        [Fact]
        public async Task CanLinkAnyTypeOfChannel()
        {
            var channel1 = new CloudEventsDataflowChannel("first");
            var channel2 = new CustomChannel();

            channel1.Subscribe(channel2);

            await channel1.Send(new InvoiceCreated());

            await channel1.DisposeAsync();
            
            Assert.True(channel2.MessageReceived);
        }
        
        private class CustomChannel : IChannel
        {
            public bool MessageReceived { get; set; } = false;
            public string Name { get; } = "mychannel";
            public Task<bool> Send(object cloudEvent)
            {
                MessageReceived = true;

                return Task.FromResult(true);
            }
        }
    }
}
