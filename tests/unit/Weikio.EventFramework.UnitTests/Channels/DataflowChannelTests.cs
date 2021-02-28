using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway;
using Xunit;

namespace Weikio.EventFramework.UnitTests.Channels
{
    public class DataflowChannelTests
    {
        [Fact]
        public void CanCreateChannel()
        {
            var channel = new DataflowChannel();
        }

        [Fact]
        public void ChannelNameDefaultsToDefaultName()
        {
            var channel = new DataflowChannel();

            Assert.Equal(ChannelName.Default, channel.Name);
        }

        [Fact]
        public void CanCreateChannelWithName()
        {
            var channel = new DataflowChannel("channelName");

            Assert.Equal("channelName", channel.Name);
        }

        [Fact]
        public void CanNotCreateChannelWithNullName()
        {
            Assert.Throws<ArgumentNullException>(() => new DataflowChannel(name: null));
        }

        [Fact]
        public async Task CanSendCloudEventToChannel()
        {
            var channel = new DataflowChannel();
            await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
        }

        [Fact]
        public async Task CanSendObjectToChannel()
        {
            var channel = new DataflowChannel();
            await channel.Send(new InvoiceCreated());
        }

        [Fact]
        public async Task BatchOfEventsIsSplit()
        {
            var counter = 0;
            var evs = CreateEvents();

            await using (var channel = new DataflowChannel("name", ev =>
            {
                counter += 1;
            }))
            {
                await channel.Send(evs);
            }

            Assert.Equal(500, counter);
        }

        [Fact]
        public async Task BatchOfObjectIsSplit()
        {
            var counter = 0;
            var evs = CreateObjects();

            await using (var channel = new DataflowChannel("name", ev =>
            {
                counter += 1;
            }))
            {
                await channel.Send(evs);
            }

            Assert.Equal(500, counter);
        }

        [Fact]
        public async Task ChannelProcessesEventsInOrder()
        {
            var evs = CreateObjects();

            var firstIndex = -1;
            var lastIndex = -1;
            var mylock = "_";

            await using (var channel = new DataflowChannel("name", ev =>
            {
                var index = ((InvoiceCreated) ev.Data).Index;

                if (firstIndex < 0)
                {
                    lock (mylock)
                    {
                        if (firstIndex < 0)
                        {
                            firstIndex = index;
                        }
                    }
                }

                lastIndex = index;
            }))
            {
                foreach (var ev in evs)
                {
                    await channel.Send(ev);
                }
            }

            Assert.Equal(0, firstIndex);
            Assert.Equal(499, lastIndex);
        }

        [Fact]
        public void CanSetEndpoint()
        {
            var channel = new DataflowChannel("name", ev => { });
        }

        [Fact]
        public async Task SentMessageEndsInEndpoint()
        {
            var counter = 0;

            using (var channel = new DataflowChannel("name", ev =>
            {
                counter += 1;
            }))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task SentObjectEndsInEndpoint()
        {
            var counter = 0;

            await using (var channel = new DataflowChannel("name", ev =>
            {
                counter += 1;
            }))
            {
                await channel.Send(new InvoiceCreated());
            }

            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task SendingMessageToChannelWithoutEndpointDoesNotThrow()
        {
            using var channel = new DataflowChannel("name");

            await channel.Send(new InvoiceCreated());
        }

        [Fact]
        public async Task SentObjectIsTransformedToCloudEvent()
        {
            object receivedEvent = null;

            using (var channel = new DataflowChannel("name", ev =>
            {
                receivedEvent = ev;
            }))
            {
                await channel.Send(new InvoiceCreated());
            }

            Assert.Equal(typeof(CloudEvent), receivedEvent.GetType());
        }

        [Fact]
        public async Task SentObjectsEndsInEndpoint()
        {
            var counter = 0;

            await using (var channel = new DataflowChannel("name", ev =>
            {
                counter += 1;
            }))
            {
                for (var i = 0; i < 500; i++)
                {
                    await channel.Send(new InvoiceCreated());
                }
            }

            Assert.Equal(500, counter);
        }

        [Fact]
        public async Task SentObjectsFromMultipleThreadsAreAllHandled()
        {
            var counter = 0;

            await using (var channel = new DataflowChannel("name", ev =>
            {
                counter += 1;
            }))
            {
                var threads = new Thread[5];

                for (var i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(async o =>
                    {
                        for (var m = 0; m < 100; m++)
                        {
                            await channel.Send(new InvoiceCreated());
                        }
                    });
                }

                foreach (var thread in threads)
                {
                    thread.Start();
                }

                foreach (var thread in threads)
                {
                    thread.Join();
                }
            }

            Assert.Equal(500, counter);
        }

        // Filtteröinti
        // Monien eventtien filtteröinti, vain halutut menevät läpi
        // attribuuttien lisäys
        // Pubsub
        // Pubsub unlink

        private static List<InvoiceCreated> CreateObjects()
        {
            var evs = new List<InvoiceCreated>();

            for (var i = 0; i < 500; i++)
            {
                evs.Add(new InvoiceCreated() { Index = i });
            }

            return evs;
        }

        private static List<CloudEvent> CreateEvents()
        {
            return CreateObjects().Select(x => CloudEventCreator.Create(x)).ToList();
        }
    }
}
