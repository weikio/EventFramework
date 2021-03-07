using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels.Dataflow.CloudEvents;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.Channels.Dataflow.UnitTests
{
    public class DataflowChannelTests : TestBase
    {
        public DataflowChannelTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CanCreateChannel()
        {
            var channel = new CloudEventsChannel();
        }

        [Fact]
        public void ChannelNameDefaultsToDefaultName()
        {
            var channel = new CloudEventsChannel();

            Assert.Equal(ChannelName.Default, channel.Name);
        }

        [Fact]
        public void CanCreateChannelWithName()
        {
            var channel = new CloudEventsChannel("channelName");

            Assert.Equal("channelName", channel.Name);
        }

        [Fact]
        public void CanNotCreateChannelWithNullName()
        {
            Assert.Throws<ArgumentNullException>(() => new CloudEventsChannel(name: null));
        }

        [Fact]
        public async Task CanSendCloudEventToChannel()
        {
            var channel = new CloudEventsChannel();
            await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
        }

        [Fact]
        public async Task CanSendObjectToChannel()
        {
            var channel = new CloudEventsChannel();
            await channel.Send(new InvoiceCreated());
        }

        [Fact]
        public async Task BatchOfEventsIsSplit()
        {
            var counter = 0;
            var evs = CreateEvents();

            await using (var channel = new CloudEventsChannel("name", ev =>
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

            await using (var channel = new CloudEventsChannel("name", ev =>
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

            await using (var channel = new CloudEventsChannel("name", ev =>
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
            var channel = new CloudEventsChannel("name", ev => { });
        }

        [Fact]
        public async Task SentMessageEndsInEndpoint()
        {
            var counter = 0;

            using (var channel = new CloudEventsChannel("name", ev =>
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

            await using (var channel = new CloudEventsChannel("name", ev =>
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
            using (var channel = new CloudEventsChannel("name"))
            {
                await channel.Send(new InvoiceCreated());
            }
        }

        [Fact]
        public async Task SentObjectIsTransformedToCloudEvent()
        {
            object receivedEvent = null;

            using (var channel = new CloudEventsChannel("name", ev =>
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

            await using (var channel = new CloudEventsChannel("name", ev =>
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
        public async Task CanConfigureCloudEventCreator()
        {
            var options = new CloudEventsDataflowChannelOptions()
            {
                Name = "test",
                CloudEventCreationOptions = new CloudEventCreationOptions()
                {
                    Subject = "changed"
                }
            };

            options.Endpoints.Add((Receive, null));
            var receivedSubject = "";
            Task Receive(CloudEvent ev)
            {
                receivedSubject = ev.Subject;
                return Task.CompletedTask;
            }

            await using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(new InvoiceCreated());
            }
            
            Assert.Equal("changed", receivedSubject);
        }
        
        [Fact]
        public async Task CanConfigureCloudEventCreatorForBatch()
        {
            var options = new CloudEventsDataflowChannelOptions()
            {
                Name = "test",
                CloudEventCreationOptions = new CloudEventCreationOptions()
                {
                    Subject = "changed"
                }
            };

            var receivedSubjects = new List<string>();
            options.Endpoints.Add((Receive, null));
            
            Task Receive(CloudEvent ev)
            {
                receivedSubjects.Add(ev.Subject);
                return Task.CompletedTask;
            }

            await using (var channel = new CloudEventsChannel(options))
            {
                var objs = CreateObjects(50);
                await channel.Send(objs);
            }

            foreach (var sub in receivedSubjects)
            {
                Assert.Equal("changed", sub);
            }
        }

        [Fact]
        public async Task SentObjectsFromMultipleThreadsAreAllHandled()
        {
            var counter = 0;

            await using (var channel = new CloudEventsChannel("name", ev =>
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

        [Fact(Skip = "Performance")]
        public async Task SentMassiveAmountsOfObjectsFromMultipleThreadsAreAllHandled()
        {
            var counter = 0;

            await using (var channel = new CloudEventsChannel("name", ev =>
            {
                counter += 1;
            }))
            {
                var threads = new Thread[5];

                for (var i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(async o =>
                    {
                        var objs = CreateManyObjects();
                        await channel.Send(objs);
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

            Assert.Equal(250000, counter);
        }

        [Fact]
        public async Task BatchOfObjectsContainSequence()
        {
            var currentIndex = 0;
            var evs = CreateObjects();

            await using (var channel = new CloudEventsChannel("name", ev =>
            {
                var attributes = ev.GetAttributes();
                var sequence = int.Parse(attributes["sequence"].ToString() ?? string.Empty);

                Assert.Equal(currentIndex + 1, sequence);
                currentIndex += 1;
            }))
            {
                await channel.Send(evs);
            }
        }

        [Fact]
        public async Task BatchOfEventsContainSequence()
        {
            var currentIndex = 0;
            var evs = CreateEvents();

            await using (var channel = new CloudEventsChannel("name", ev =>
            {
                var attributes = ev.GetAttributes();
                var sequence = int.Parse(attributes["sequence"].ToString() ?? string.Empty);

                Assert.Equal(currentIndex + 1, sequence);
                currentIndex += 1;
            }))
            {
                await channel.Send(evs);
            }
        }

        [Fact]
        public async Task SequenceStartsFromOne()
        {
            var evs = CreateObjects();
            var seqs = new List<int>();

            await using (var channel = new CloudEventsChannel("name", ev =>
            {
                var attributes = ev.GetAttributes();
                var sequence = int.Parse(attributes["sequence"].ToString() ?? string.Empty);

                seqs.Add(sequence);
            }))
            {
                await channel.Send(evs);
            }

            Assert.Equal(1, seqs.Min());
            Assert.Equal(500, seqs.Max());
        }

        [Fact]
        public async Task CanCreateChannelUsingOptions()
        {
            var counter = 0;

            var options = new CloudEventsDataflowChannelOptions()
            {
                Name = "name",
                Endpoint = ev =>
                {
                    counter += 1;
                },
                LoggerFactory = _loggerFactory
            };

            await using (var channel = new CloudEventsChannel(options))
            {
                Assert.Equal("name", channel.Name);

                for (var i = 0; i < 500; i++)
                {
                    await channel.Send(new InvoiceCreated());
                }
            }

            Assert.Equal(500, counter);
        }

        [Fact]
        public async Task CanAddComponent()
        {
            var counter = 0;

            var options = new CloudEventsDataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            options.Components.Add(new Func<CloudEvent, CloudEvent>(ev =>
            {
                counter += 1;

                return ev;
            }));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task CanAddComponentWithPredicate()
        {
            var counter = 0;

            var options = new CloudEventsDataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            CloudEvent AddCounter(CloudEvent cloudEvent)
            {
                counter += 1;

                return cloudEvent;
            }

            bool ShouldRunComponent(CloudEvent cloudEvent)
            {
                var inv = CloudEvent<InvoiceCreated>.Create(cloudEvent);

                if (inv.Object.Index == 0)
                {
                    return false;
                }

                return true;
            }

            options.Components.Add((AddCounter, ShouldRunComponent));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 5 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 7 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(2, counter);
        }

        [Fact]
        public async Task CanAddComponentWithPredicateIntoBeginning()
        {
            var counter = 0;

            var options = new CloudEventsDataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            CloudEvent AddCounter(CloudEvent cloudEvent)
            {
                counter += 1;

                return cloudEvent;
            }

            bool ShouldRunComponent(CloudEvent cloudEvent)
            {
                var inv = CloudEvent<InvoiceCreated>.Create(cloudEvent);

                if (inv.Object.Index == 0)
                {
                    return false;
                }

                return true;
            }

            options.Components.Add((AddCounter, ShouldRunComponent));
            options.Components.Add((AddCounter, null));
            options.Components.Add((AddCounter, null));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 5 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 7 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(12, counter);
        }

        [Fact]
        public async Task CanAddComponentWithPredicateIntoMiddle()
        {
            var counter = 0;

            var options = new CloudEventsDataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            CloudEvent AddCounter(CloudEvent cloudEvent)
            {
                counter += 1;

                return cloudEvent;
            }

            bool ShouldRunComponent(CloudEvent cloudEvent)
            {
                var inv = CloudEvent<InvoiceCreated>.Create(cloudEvent);

                if (inv.Object.Index == 0)
                {
                    return false;
                }

                return true;
            }

            options.Components.Add((AddCounter, null));
            options.Components.Add((AddCounter, ShouldRunComponent));
            options.Components.Add((AddCounter, null));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 5 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 7 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(12, counter);
        }

        [Fact]
        public async Task CanAddComponentWithPredicateIntoEnd()
        {
            var counter = 0;

            var options = new CloudEventsDataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            CloudEvent AddCounter(CloudEvent cloudEvent)
            {
                counter += 1;

                return cloudEvent;
            }

            bool ShouldRunComponent(CloudEvent cloudEvent)
            {
                var inv = CloudEvent<InvoiceCreated>.Create(cloudEvent);

                if (inv.Object.Index == 0)
                {
                    return false;
                }

                return true;
            }

            options.Components.Add((AddCounter, null));
            options.Components.Add((AddCounter, null));
            options.Components.Add((AddCounter, ShouldRunComponent));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 5 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 7 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(12, counter);
        }

        [Fact]
        public async Task CanAddComponents()
        {
            var counter = 0;

            var options = new CloudEventsDataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            options.Components.Add(new Func<CloudEvent, CloudEvent>(ev =>
            {
                counter += 1;

                return ev;
            }));

            options.Components.Add(new Func<CloudEvent, CloudEvent>(ev =>
            {
                counter += 5;

                return ev;
            }));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(6, counter);
        }

        [Fact]
        public async Task ComponentsAreRunInOrder()
        {
            List<int> counters = new();

            var options = new CloudEventsDataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                counters.Add(1);

                return ev;
            }));

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                counters.Add(5);

                return ev;
            }));

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                counters.Add(100);

                return ev;
            }));

            await using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(1, counters[0]);
            Assert.Equal(5, counters[1]);
            Assert.Equal(100, counters[2]);
        }

        [Fact]
        public async Task CrashedComponentDoesntBreakChannel()
        {
            var counter = 0;
            var hasCrashed = false;

            var options = new CloudEventsDataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                counter += 1;

                return ev;
            }));

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                if (!hasCrashed)
                {
                    hasCrashed = true;

                    throw new Exception();
                }

                counter += 5;

                return ev;
            }));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(7, counter);
        }

        [Fact]
        public async Task CrashedComponentBreaksTheMessageFlow()
        {
            var counter = 0;
            var hasCrashed = false;

            var options = new CloudEventsDataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                return ev;
            }));

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                if (!hasCrashed)
                {
                    hasCrashed = true;

                    throw new Exception();
                }

                return ev;
            }));

            options.Endpoint = ev =>
            {
                counter += 1;
            };

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            // Component1 --> Component2 Crash --> Null
            // Component1 --> Component2 --> Endpoint

            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task CanAddFilter()
        {
            var counter = 0;

            var options = new CloudEventsDataflowChannelOptions()
            {
                Name = "name",
                Endpoint = ev =>
                {
                    counter += 1;
                },
                LoggerFactory = _loggerFactory
            };

            options.Components.Add(new CloudEventsComponent(ev => null));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(0, counter);
        }

        [Fact]
        public async Task CanAddTransform()
        {
            var options = new CloudEventsDataflowChannelOptions()
            {
                Name = "name",
                Endpoint = ev =>
                {
                    Assert.Equal("transformed", ev.Subject);
                },
                LoggerFactory = _loggerFactory
            };

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                ev.Subject = "transformed";

                return ev;
            }));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }
        }

        [Fact]
        public async Task CanAddMultipleTransforms()
        {
            var options = new CloudEventsDataflowChannelOptions()
            {
                Name = "name",
                Endpoint = ev =>
                {
                    Assert.Equal("111", ev.Subject);
                },
                LoggerFactory = _loggerFactory
            };

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                ev.Subject = (ev.Subject ?? "") + "1";

                return ev;
            }));

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                ev.Subject = (ev.Subject ?? "") + "1";

                return ev;
            }));

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                ev.Subject = (ev.Subject ?? "") + "1";

                return ev;
            }));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }
        }

        [Fact]
        public async Task CanAddMultipleFilters()
        {
            var counter = 0;

            var options = new CloudEventsDataflowChannelOptions()
            {
                Name = "name",
                Endpoint = ev =>
                {
                    counter += 1;
                },
                LoggerFactory = _loggerFactory
            };

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                var typedEvent = CloudEvent<InvoiceCreated>.Create(ev);

                if (typedEvent.Object.Index < 5)
                {
                    return null;
                }

                return ev;
            }));

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                var typedEvent = CloudEvent<InvoiceCreated>.Create(ev);

                if (typedEvent.Object.Index < 50)
                {
                    return null;
                }

                return ev;
            }));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 4 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 10 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 100 }));
            }

            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task CanAddFilterWhichTargetsOnlySomeOfTheMessages()
        {
            var counter = 0;

            var options = new CloudEventsDataflowChannelOptions()
            {
                Name = "name",
                Endpoint = ev =>
                {
                    counter += 1;
                },
            };

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                var typedEvent = CloudEvent<InvoiceCreated>.Create(ev);

                if (typedEvent.Object.Index < 5)
                {
                    return null;
                }

                return ev;
            }));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 4 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 10 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 100 }));
            }

            Assert.Equal(2, counter);
        }

        [Fact]
        public async Task CanAddMultipleEndpoints()
        {
            var options = new CloudEventsDataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            var counter = 0;

            options.Endpoints.Add(new CloudEventsEndpoint(ev =>
            {
                counter += 1;

                return Task.CompletedTask;
            }));

            options.Endpoints.Add(new CloudEventsEndpoint(ev =>
            {
                counter += 1;

                return Task.CompletedTask;
            }));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(2, counter);
        }

        [Fact]
        public async Task CanAddEndpointWithPredicate()
        {
            var options = new CloudEventsDataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            var counter = 0;

            options.Endpoints.Add(new CloudEventsEndpoint(ev =>
            {
                counter += 1;

                return Task.CompletedTask;
            }, ev =>
            {
                var inv = ev.To<InvoiceCreated>();

                if (inv.Object.Index == 0)
                {
                    return false;
                }

                return true;
            }));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 1 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task CanAddEndpointWithPredicateIntoBeginning()
        {
            var options = new CloudEventsDataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            var counter = 0;

            Task Action(CloudEvent ev)
            {
                counter += 1;

                return Task.CompletedTask;
            }

            bool Predicate(CloudEvent ev)
            {
                var inv = ev.To<InvoiceCreated>();

                if (inv.Object.Index == 0)
                {
                    return false;
                }

                return true;
            }

            options.Endpoints.Add((Action, Predicate));
            options.Endpoints.Add((Action, null));
            options.Endpoints.Add((Action, null));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 1 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(7, counter);
        }

        [Fact]
        public async Task CanAddEndpointWithPredicateIntoMiddle()
        {
            var options = new CloudEventsDataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            var counter = 0;

            Task Action(CloudEvent ev)
            {
                counter += 1;

                return Task.CompletedTask;
            }

            bool Predicate(CloudEvent ev)
            {
                var inv = ev.To<InvoiceCreated>();

                if (inv.Object.Index == 0)
                {
                    return false;
                }

                return true;
            }

            options.Endpoints.Add((Action, null));
            options.Endpoints.Add((Action, Predicate));
            options.Endpoints.Add((Action, null));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 1 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(7, counter);
        }

        [Fact]
        public async Task CanAddEndpointWithPredicateIntoEnd()
        {
            var options = new CloudEventsDataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            var counter = 0;

            Task Action(CloudEvent ev)
            {
                counter += 1;

                return Task.CompletedTask;
            }

            bool Predicate(CloudEvent ev)
            {
                var inv = ev.To<InvoiceCreated>();

                if (inv.Object.Index == 0)
                {
                    return false;
                }

                return true;
            }

            options.Endpoints.Add((Action, null));
            options.Endpoints.Add((Action, null));
            options.Endpoints.Add((Action, Predicate));

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated() { Index = 1 }));
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(7, counter);
        }

        [Fact]
        public async Task CanAddFilterWhichTargetSomeOfTheMessagesInBatch()
        {
            var counter = 0;

            var options = new CloudEventsDataflowChannelOptions()
            {
                Name = "name",
                Endpoint = ev =>
                {
                    counter += 1;
                },
            };

            options.Components.Add(new CloudEventsComponent(ev =>
            {
                var typedEvent = CloudEvent<InvoiceCreated>.Create(ev);

                if (typedEvent.Object.Index < 50)
                {
                    return null;
                }

                return ev;
            }));

            var objects = CreateObjects();

            using (var channel = new CloudEventsChannel(options))
            {
                await channel.Send(objects);
            }

            Assert.Equal(450, counter);
        }

        [Fact]
        public async Task ChannelCanSentToAnotherChannel()
        {
            var counter = 0;

            var firstOptions = new CloudEventsDataflowChannelOptions() { Name = "first", };

            var secondOptions = new CloudEventsDataflowChannelOptions()
            {
                Name = "second",
                Endpoint = ev =>
                {
                    counter += 1;
                },
            };

            var channel2 = new CloudEventsChannel(secondOptions);

            firstOptions.Endpoints.Add(new CloudEventsEndpoint(async ev =>
            {
                await channel2.Send(ev);
            }));

            var objects = CreateManyObjects();

            var channel1 = new CloudEventsChannel(firstOptions);

            await channel1.Send(objects);

            await channel1.DisposeAsync();
            await channel2.DisposeAsync();

            Assert.Equal(50000, counter);
        }
    }
}
