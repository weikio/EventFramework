using System;
using System.Threading.Tasks;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
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

            var firstOptions = new CloudEventsChannelOptions()
            {
                Name = "first",
                Endpoint = ev =>
                {
                    firstCounter += 10;
                },
                LoggerFactory = _loggerFactory
            };

            var secondOptions = new CloudEventsChannelOptions()
            {
                Name = "second",
                Endpoint = ev =>
                {
                    secondCounter += 3;
                },
                LoggerFactory = _loggerFactory
            };

            var count = 100;
            var objects = CreateObjects(count);

            var channel1 = new CloudEventsChannel(firstOptions);
            var channel2 = new CloudEventsChannel(secondOptions);

            channel1.Subscribe(channel2);

            await channel1.Send(objects);

            await channel1.DisposeAsync();
            await channel2.DisposeAsync();

            Assert.Equal(count * 10, firstCounter);
            Assert.Equal(count * 3, secondCounter);
        }
        
        [Fact]
        public async Task ChannelCanSubscribeToAnotherChannelWithPredicate()
        {
            var firstCounter = 0;
            var secondCounter = 0;

            var firstOptions = new CloudEventsChannelOptions()
            {
                Name = "first",
                Endpoint = ev =>
                {
                    firstCounter += 1;
                },
                LoggerFactory = _loggerFactory
            };

            var secondOptions = new CloudEventsChannelOptions()
            {
                Name = "second",
                Endpoint = ev =>
                {
                    secondCounter += 1;
                },
                LoggerFactory = _loggerFactory
            };

            var channel1 = new CloudEventsChannel(firstOptions);
            var channel2 = new CloudEventsChannel(secondOptions);

            channel1.Subscribe(channel2, ev => string.Equals(ev.Type, "InvoiceCreated", StringComparison.InvariantCultureIgnoreCase));

            await channel1.Send(new InvoiceCreated());
            await channel1.Send(new InvoiceCreated());
            await channel1.Send(new CustomerCreated(Guid.NewGuid(), "test", "test"));
            await channel1.Send(new CustomerCreated(Guid.NewGuid(), "test", "test"));
            await channel1.Send(new CustomerCreated(Guid.NewGuid(), "test", "test"));
            await channel1.Send(new InvoiceCreated());
            await channel1.Send(new InvoiceCreated());

            await channel1.DisposeAsync();
            await channel2.DisposeAsync();

            Assert.Equal(7, firstCounter);
            Assert.Equal(4, secondCounter);
        }
        
        [Fact]
        public async Task ChannelIsByDefaultPubSub()
        {
            var secondReceivedMessage = false;
            var thirdReceivedMessage = false;

            var firstOptions = new CloudEventsChannelOptions()
            {
                Name = "first",
                LoggerFactory = _loggerFactory
            };

            var secondOptions = new CloudEventsChannelOptions()
            {
                Name = "second",
                Endpoint = ev =>
                {
                    secondReceivedMessage = true;
                },
                LoggerFactory = _loggerFactory
            };

            var thirdOptions = new CloudEventsChannelOptions()
            {
                Name = "third",
                Endpoint = ev =>
                {
                    thirdReceivedMessage = true;
                },
                LoggerFactory = _loggerFactory
            };

            
            var channel1 = new CloudEventsChannel(firstOptions);
            var channel2 = new CloudEventsChannel(secondOptions);
            var channel3 = new CloudEventsChannel(thirdOptions);

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

            var firstOptions = new CloudEventsChannelOptions()
            {
                Name = "first",
                LoggerFactory = _loggerFactory,
                IsPubSub = false
            };

            var secondOptions = new CloudEventsChannelOptions()
            {
                Name = "second",
                Endpoint = ev =>
                {
                    secondReceivedMessage = true;
                },
                LoggerFactory = _loggerFactory
            };

            var thirdOptions = new CloudEventsChannelOptions()
            {
                Name = "third",
                Endpoint = ev =>
                {
                    thirdReceivedMessage = true;
                },
                LoggerFactory = _loggerFactory
            };

            
            var channel1 = new CloudEventsChannel(firstOptions);
            var channel2 = new CloudEventsChannel(secondOptions);
            var channel3 = new CloudEventsChannel(thirdOptions);

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

            var firstOptions = new CloudEventsChannelOptions()
            {
                Name = "first",
                Endpoint = ev =>
                {
                    firstCounter += 10;
                },
                LoggerFactory = _loggerFactory
            };

            var secondOptions = new CloudEventsChannelOptions()
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

            var channel1 = new CloudEventsChannel(firstOptions);
            var channel2 = new CloudEventsChannel(secondOptions);

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
            var channel1 = new CloudEventsChannel("first");
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
