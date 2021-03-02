using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.UnitTests.Channels
{
    public class DataflowChannelTests
    {
        private readonly ITestOutputHelper _output;
        private ILoggerFactory _loggerFactory;

        public DataflowChannelTests(ITestOutputHelper output)
        {
            _output = output;

            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddXUnit(output);
            });
        }

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
            using (var channel = new DataflowChannel("name"))
            {
                await channel.Send(new InvoiceCreated());
            }
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
        
        [Fact]
        public async Task SentMassiveAmountsOfObjectsFromMultipleThreadsAreAllHandled()
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

            await using (var channel = new DataflowChannel("name", ev =>
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

            await using (var channel = new DataflowChannel("name", ev =>
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

            await using (var channel = new DataflowChannel("name", ev =>
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

            var options = new DataflowChannelOptions()
            {
                Name = "name",
                Endpoint = ev =>
                {
                    counter += 1;
                },
                LoggerFactory = _loggerFactory
            };

            await using (var channel = new DataflowChannel(options))
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

            var options = new DataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            options.Components.Add(ev =>
            {
                counter += 1;

                return ev;
            });

            using (var channel = new DataflowChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task CanAddComponents()
        {
            var counter = 0;

            var options = new DataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            options.Components.Add(ev =>
            {
                counter += 1;

                return ev;
            });

            options.Components.Add(ev =>
            {
                counter += 5;

                return ev;
            });

            using (var channel = new DataflowChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(6, counter);
        }

        [Fact]
        public async Task ComponentsAreRunInOrder()
        {
            List<int> counters = new();

            var options = new DataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            options.Components.Add(ev =>
            {
                counters.Add(1);

                return ev;
            });

            options.Components.Add(ev =>
            {
                counters.Add(5);

                return ev;
            });

            options.Components.Add(ev =>
            {
                counters.Add(100);

                return ev;
            });

            await using (var channel = new DataflowChannel(options))
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

            var options = new DataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            options.Components.Add(ev =>
            {
                counter += 1;

                return ev;
            });

            options.Components.Add(ev =>
            {
                if (!hasCrashed)
                {
                    hasCrashed = true;

                    throw new Exception();
                }

                counter += 5;

                return ev;
            });

            using (var channel = new DataflowChannel(options))
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

            var options = new DataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            options.Components.Add(ev =>
            {
                return ev;
            });

            options.Components.Add(ev =>
            {
                if (!hasCrashed)
                {
                    hasCrashed = true;

                    throw new Exception();
                }

                return ev;
            });

            options.Endpoint = ev =>
            {
                counter += 1;
            };

            using (var channel = new DataflowChannel(options))
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

            var options = new DataflowChannelOptions()
            {
                Name = "name",
                Endpoint = ev =>
                {
                    counter += 1;
                },
                LoggerFactory = _loggerFactory
            };

            options.Components.Add(ev => null);

            using (var channel = new DataflowChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(0, counter);
        }

        [Fact]
        public async Task CanAddTransform()
        {
            var options = new DataflowChannelOptions()
            {
                Name = "name",
                Endpoint = ev =>
                {
                    Assert.Equal("transformed", ev.Subject);
                },
                LoggerFactory = _loggerFactory
            };

            options.Components.Add(ev =>
            {
                ev.Subject = "transformed";

                return ev;
            });

            using (var channel = new DataflowChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }
        }

        [Fact]
        public async Task CanAddMultipleTransforms()
        {
            var options = new DataflowChannelOptions()
            {
                Name = "name",
                Endpoint = ev =>
                {
                    Assert.Equal("111", ev.Subject);
                },
                LoggerFactory = _loggerFactory
            };

            options.Components.Add(ev =>
            {
                ev.Subject = (ev.Subject ?? "") + "1";

                return ev;
            });

            options.Components.Add(ev =>
            {
                ev.Subject = (ev.Subject ?? "") + "1";

                return ev;
            });

            options.Components.Add(ev =>
            {
                ev.Subject = (ev.Subject ?? "") + "1";

                return ev;
            });

            using (var channel = new DataflowChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }
        }

        [Fact]
        public async Task CanAddMultipleFilters()
        {
            var counter = 0;

            var options = new DataflowChannelOptions()
            {
                Name = "name",
                Endpoint = ev =>
                {
                    counter += 1;
                },
                LoggerFactory = _loggerFactory
            };

            options.Components.Add(ev =>
            {
                var typedEvent = CloudEvent<InvoiceCreated>.Create(ev);

                if (typedEvent.Object.Index < 5)
                {
                    return null;
                }

                return ev;
            });

            options.Components.Add(ev =>
            {
                var typedEvent = CloudEvent<InvoiceCreated>.Create(ev);

                if (typedEvent.Object.Index < 50)
                {
                    return null;
                }

                return ev;
            });

            using (var channel = new DataflowChannel(options))
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

            var options = new DataflowChannelOptions()
            {
                Name = "name",
                Endpoint = ev =>
                {
                    counter += 1;
                },
            };

            options.Components.Add(ev =>
            {
                var typedEvent = CloudEvent<InvoiceCreated>.Create(ev);

                if (typedEvent.Object.Index < 5)
                {
                    return null;
                }

                return ev;
            });

            using (var channel = new DataflowChannel(options))
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
            var options = new DataflowChannelOptions() { Name = "name", LoggerFactory = _loggerFactory };

            var counter = 0;

            options.Endpoints.Add(ev =>
            {
                counter += 1;

                return Task.CompletedTask;
            });

            options.Endpoints.Add(ev =>
            {
                counter += 1;

                return Task.CompletedTask;
            });

            using (var channel = new DataflowChannel(options))
            {
                await channel.Send(CloudEventCreator.Create(new InvoiceCreated()));
            }

            Assert.Equal(2, counter);
        }

        [Fact]
        public async Task CanAddFilterWhichTargetSomeOfTheMessagesInBatch()
        {
            var counter = 0;

            var options = new DataflowChannelOptions()
            {
                Name = "name",
                Endpoint = ev =>
                {
                    counter += 1;
                },
            };

            options.Components.Add(ev =>
            {
                var typedEvent = CloudEvent<InvoiceCreated>.Create(ev);

                if (typedEvent.Object.Index < 50)
                {
                    return null;
                }

                return ev;
            });

            var objects = CreateObjects();

            using (var channel = new DataflowChannel(options))
            {
                await channel.Send(objects);
            }

            Assert.Equal(450, counter);
        }

        [Fact]
        public async Task ChannelCanSubscribeToAnotherChannel()
        {
            var firstCounter = 0;
            var secondCounter = 0;

            var firstOptions = new DataflowChannelOptions()
            {
                Name = "first",
                Endpoint = ev =>
                {
                    firstCounter += 10;
                },
                LoggerFactory = _loggerFactory
            };

            var secondOptions = new DataflowChannelOptions()
            {
                Name = "second",
                Endpoint = ev =>
                {
                    secondCounter += 3;
                },
                LoggerFactory = _loggerFactory
            };

            var count = 1000000;
            var objects = CreateObjects(count);

            var channel1 = new DataflowChannel(firstOptions);
            var channel2 = new DataflowChannel(secondOptions);

            channel1.Subscribe(channel2);

            await channel1.Send(objects);

            await channel1.DisposeAsync();
            await channel2.DisposeAsync();

            Assert.Equal(count * 10, firstCounter);
            Assert.Equal(count * 3, secondCounter);
        }

        [Fact]
        public async Task ChannelCanUnsubscribeFromAnotherChannel()
        {
            var firstCounter = 0;
            var secondCounter = 0;

            var firstOptions = new DataflowChannelOptions()
            {
                Name = "first",
                Endpoint = ev =>
                {
                    firstCounter += 10;
                },
                LoggerFactory = _loggerFactory
            };

            var secondOptions = new DataflowChannelOptions()
            {
                Name = "second",
                Endpoint = ev =>
                {
                    secondCounter += 3;
                },
                LoggerFactory = _loggerFactory
            };

            var count = 10000;
            var objects = CreateObjects(count);

            var channel1 = new DataflowChannel(firstOptions);
            var channel2 = new DataflowChannel(secondOptions);

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
        public async Task ChannelCanSentToAnotherChannel()
        {
            var counter = 0;

            var firstOptions = new DataflowChannelOptions() { Name = "first", };

            var secondOptions = new DataflowChannelOptions()
            {
                Name = "second",
                Endpoint = ev =>
                {
                    counter += 1;
                },
            };

            var channel2 = new DataflowChannel(secondOptions);

            firstOptions.Endpoints.Add(async ev =>
            {
                await channel2.Send(ev);
            });

            var objects = CreateManyObjects();

            var channel1 = new DataflowChannel(firstOptions);

            await channel1.Send(objects);

            await channel1.DisposeAsync();
            await channel2.DisposeAsync();

            Assert.Equal(50000, counter);
        }

        // Pubsub
        // Pubsub unlink

        private static List<InvoiceCreated> CreateObjects(int count = 500)
        {
            var evs = new List<InvoiceCreated>();

            for (var i = 0; i < count; i++)
            {
                evs.Add(new InvoiceCreated() { Index = i });
            }

            return evs;
        }
        
        private static List<InvoiceCreated> CreateManyObjects()
        {
            return CreateObjects(50000);
        }

        private static List<CloudEvent> CreateEvents()
        {
            return CreateObjects().Select(x => CloudEventCreator.Create(x)).ToList();
        }

        private async Task ContinueWhen(Func<bool> probe, string assertErrorMessage = null, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromSeconds(3);
            }

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout.GetValueOrDefault());

            var success = false;

            while (cts.IsCancellationRequested == false)
            {
                success = probe();

                if (success)
                {
                    break;
                }

                if (cts.IsCancellationRequested)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }
        }
    }
}
