using System;
using System.Linq;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.EventSourceWrapping;
using Weikio.EventFramework.IntegrationTests.EventSource.Sources;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.EventSource
{
    [Collection(nameof(NotThreadSafeResourceCollection))]
    public class PollingEventSourceTests : PollingEventSourceTestBase, IDisposable
    {
        public PollingEventSourceTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task CanStart()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventDataflowChannels();
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<StatelessTestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "StatelessTestEventSource", };

            await eventSourceInstanceManager.Create(options);

            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task CanUseConfiguration()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1),
                configuration: new TestEsConfiguration() { ExtraFile = "fromConfig.test" });

            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(2));

            var instanceFile = MyTestCloudEventPublisher.PublishedEvents.Select(CloudEvent<NewFileEvent>.Create)
                .FirstOrDefault(x => x.Object.FileName == "fromConfig.test");
            Assert.NotNull(instanceFile);
        }

        [Fact]
        public async Task CanAutoStartStateles()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<StatelessTestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            var options = new EventSourceInstanceOptions()
            {
                PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "StatelessTestEventSource", Autostart = true
            };

            await eventSourceInstanceManager.Create(options);

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task CanAutostart()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            var options = new EventSourceInstanceOptions()
            {
                Autostart = true, PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "TestEventSource"
            };

            await eventSourceInstanceManager.Create(options);

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task CanSetInitialStateOnFirstRun()
        {
            var server = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<StatefulEventSourceWithInitialization>();

                services.AddSingleton(new EventSourceInstanceOptions()
                {
                    Autostart = true, PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "StatefulEventSourceWithInitialization"
                });
            });

            await Task.Delay(TimeSpan.FromSeconds(3));

            Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);

            for (var i = 0; i < MyTestCloudEventPublisher.PublishedEvents.Count - 1; i++)
            {
                Assert.Equal($"{i + 10}.txt", CloudEvent<NewFileEvent>.Create(MyTestCloudEventPublisher.PublishedEvents[i]).Object.FileName);
            }
        }

        [Fact]
        public async Task MultiplePollersCanHaveDifferentFrequency()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();

                services.AddChannel("bus", (provider, options) =>
                {
                });

                services.Configure<DefaultChannelOptions>(options =>
                {
                    options.DefaultChannelName = "bus";
                });
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configure: new Action<TestEventSource>(source =>
            {
                source.ExtraFile = "first.test";
            }));

            await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(10), configure: new Action<TestEventSource>(source =>
            {
                source.ExtraFile = "another.test";
            }));

            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(2));

            var allEvents = MyTestCloudEventPublisher.PublishedEvents.Select(CloudEvent<NewFileEvent>.Create).ToList();

            var firstFileEvent = allEvents.FirstOrDefault(x => x.Object.FileName == "first.test");
            var anotherFileEvent = allEvents.FirstOrDefault(x => x.Object.FileName == "another.test");

            Assert.NotNull(firstFileEvent);
            Assert.Null(anotherFileEvent);
        }

        [Fact]
        public async Task CanConfigure()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configure: new Action<TestEventSource>(source =>
            {
                source.ExtraFile = "first.test";
            }));

            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(2));

            var instanceFile = MyTestCloudEventPublisher.PublishedEvents.Select(CloudEvent<NewFileEvent>.Create)
                .FirstOrDefault(x => x.Object.FileName == "first.test");
            Assert.NotNull(instanceFile);
        }

        [Fact]
        public async Task StatelessIsNotAutoStartedByDefault()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<StatelessTestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "StatelessTestEventSource", };

            await eventSourceInstanceManager.Create(options);

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.Empty(MyTestCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task CanRunOnce()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<StatelessTestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            var options = new EventSourceInstanceOptions()
            {
                PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "StatelessTestEventSource", RunOnce = true
            };

            await eventSourceInstanceManager.Create(options);

            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(3));

            Assert.Single(MyTestCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task SameInstanceIsUsedBetweenRuns()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<StatefulEventSource>();

                services.Configure<EventSourceInstanceOptions>(options =>
                {
                    options.PollingFrequency = TimeSpan.FromSeconds(1);
                    options.EventSourceDefinition = "StatefulEventSource";
                    options.Autostart = true;
                });
            });

            await Task.Delay(TimeSpan.FromSeconds(3));

            for (var i = 0; i < MyTestCloudEventPublisher.PublishedEvents.Count - 1; i++)
            {
                Assert.Equal($"{i}.txt", CloudEvent<NewFileEvent>.Create(MyTestCloudEventPublisher.PublishedEvents[i]).Object.FileName);
            }
        }

        [Fact]
        public async Task TestStatusLifecycle()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<StatelessTestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "StatelessTestEventSource" };

            var id = await eventSourceInstanceManager.Create(options);
            var instance = eventSourceInstanceManager.Get(id);

            await eventSourceInstanceManager.StartAll();
            await Task.Delay(TimeSpan.FromSeconds(2));
            await eventSourceInstanceManager.StopAll();
            await Task.Delay(TimeSpan.FromSeconds(2));
            await eventSourceInstanceManager.RemoveAll();
            await Task.Delay(TimeSpan.FromSeconds(2));

            var instanceStatus = instance.Status;
            Assert.Equal(EventSourceStatusEnum.New, instanceStatus.Messages[0].NewStatus);
            Assert.Equal(EventSourceStatusEnum.Starting, instanceStatus.Messages[1].NewStatus);
            Assert.Equal(EventSourceStatusEnum.Started, instanceStatus.Messages[2].NewStatus);
            Assert.Equal(EventSourceStatusEnum.Stopping, instanceStatus.Messages[3].NewStatus);
            Assert.Equal(EventSourceStatusEnum.Stopped, instanceStatus.Messages[4].NewStatus);
            Assert.Equal(EventSourceStatusEnum.Removed, instanceStatus.Messages[5].NewStatus);
        }

        [Fact]
        public async Task CanCreateAutoStartInstanceInConfigure()
        {
            Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();

                services.AddSingleton(new EventSourceInstanceOptions()
                {
                    Autostart = true, PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "TestEventSource"
                });
            });

            await ContinueWhen(MyTestCloudEventPublisher.PublishedEvents.Any);
        }

        [Fact]
        public async Task CanCreateInstanceInConfigure()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();

                services.AddSingleton(new EventSourceInstanceOptions()
                {
                    Autostart = false, PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "TestEventSource"
                });
            });

            var instances = serviceProvider.GetRequiredService<IEventSourceInstanceManager>().GetAll();
            Assert.Single(instances);
        }

        [Fact]
        public async Task CanConfigureEventCreationOptions()
        {
            throw new NotImplementedException();
            //
            // var serviceProvider = Init(services =>
            // {
            //     services.AddCloudEventSources();
            //     services.AddCloudEventPublisher();
            //     services.AddLocal();
            //     services.AddEventSource<TestEventSource>();
            // });
            //
            // var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            //
            // var id = await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configureDefaultCloudEventCreationOptions: options =>
            // {
            //     options.Subject = "mytest";
            // });
            //
            // await eventSourceInstanceManager.StartAll();
            //
            // await Task.Delay(TimeSpan.FromSeconds(2));
            //
            // Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);
            //
            // foreach (var ev in MyTestCloudEventPublisher.PublishedEvents)
            // {
            //     Assert.Equal("mytest", ev.Subject);
            // }
        }

        [Fact]
        public async Task CanConfigureDefaultEventCreationOptions()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();

                services.Configure<CloudEventCreationOptions>(options =>
                {
                    options.Subject = "another";
                });
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1));

            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);

            foreach (var ev in MyTestCloudEventPublisher.PublishedEvents)
            {
                Assert.Equal("another", ev.Subject);
            }
        }

        [Fact]
        public async Task CanConfigureEventTypeSpecificEventCreationOptions()
        {
            throw new NotImplementedException();
            // var serviceProvider = Init(services =>
            // {
            //     services.AddCloudEventSources();
            //     services.AddCloudEventPublisher();
            //     services.AddLocal();
            //     services.AddEventSource<TestEventSource>();
            //
            //     services.Configure<CloudEventPublisherOptions>(options =>
            //     {
            //         options.TypedCloudEventCreationOptions.Add("NewFileEvent", creationOptions =>
            //         {
            //             creationOptions.Subject = "specific";
            //         });
            //     });
            // });
            //
            // var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            // await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1));
            //
            // await eventSourceInstanceManager.StartAll();
            //
            // await Task.Delay(TimeSpan.FromSeconds(2));
            //
            // Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);
            //
            // foreach (var ev in MyTestCloudEventPublisher.PublishedEvents)
            // {
            //     Assert.Equal("specific", ev.Subject);
            // }
        }

        [Fact]
        public async Task CanConfigureEventTypeSpecificEventCreationOptionsForInstance()
        {
            throw new NotImplementedException();

            // var serviceProvider = Init(services =>
            // {
            //     services.AddCloudEventSources();
            //     services.AddCloudEventPublisher();
            //     services.AddLocal();
            //     services.AddEventSource<TestEventSource>();
            //
            //     services.Configure<CloudEventPublisherOptions>(options =>
            //     {
            //         options.TypedCloudEventCreationOptions.Add("NewFileEvent", creationOptions =>
            //         {
            //             creationOptions.Subject = "default";
            //         });
            //     });
            // });
            //
            // var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            //
            // await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configurePublisherOptions: options =>
            // {
            //     options.TypedCloudEventCreationOptions["NewFileEvent"] = creationOptions =>
            //     {
            //         creationOptions.Subject = "instance";
            //     };
            // });
            //
            // await eventSourceInstanceManager.StartAll();
            //
            // await Task.Delay(TimeSpan.FromSeconds(2));
            //
            // Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);
            //
            // foreach (var ev in MyTestCloudEventPublisher.PublishedEvents)
            // {
            //     Assert.Equal("instance", ev.Subject);
            // }
        }

        [Fact]
        public async Task CanOverrideEventTypeSpecificEventCreationOptionsForInstance()
        {
            throw new NotImplementedException();
            //
            // var serviceProvider = Init(services =>
            // {
            //     services.AddCloudEventSources();
            //     services.AddCloudEventPublisher();
            //     services.AddLocal();
            //     services.AddEventSource<TestEventSource>();
            // });
            //
            // var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            //
            // await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configurePublisherOptions: options =>
            // {
            //     options.TypedCloudEventCreationOptions.Add("NewFileEvent", creationOptions =>
            //     {
            //         creationOptions.Subject = "instance";
            //     });
            // });
            //
            // await eventSourceInstanceManager.StartAll();
            //
            // await Task.Delay(TimeSpan.FromSeconds(2));
            //
            // Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);
            //
            // foreach (var ev in MyTestCloudEventPublisher.PublishedEvents)
            // {
            //     Assert.Equal("instance", ev.Subject);
            // }
        }

        [Fact]
        public async Task CanConfigureInstanceSpecificCreationOptions()
        {
            throw new NotImplementedException();
            // var serviceProvider = Init(services =>
            // {
            //     services.AddCloudEventSources();
            //     services.AddCloudEventPublisher();
            //     services.AddLocal();
            //     services.AddEventSource<TestEventSource>();
            // });
            //
            // var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            //
            // var id = await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configureDefaultCloudEventCreationOptions: options =>
            // {
            //     options.Subject = "mytest";
            // });
            //
            // var id2 = await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configureDefaultCloudEventCreationOptions: options =>
            // {
            //     options.Subject = "anothertest";
            // });
            //
            // await eventSourceInstanceManager.StartAll();
            //
            // await Task.Delay(TimeSpan.FromSeconds(2));
            //
            // Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);
            //
            // var id1Found = false;
            // var id2Found = false;
            //
            // foreach (var ev in MyTestCloudEventPublisher.PublishedEvents)
            // {
            //     if (ev.EventSourceId() == id)
            //     {
            //         Assert.Equal("mytest", ev.Subject);
            //         id1Found = true;
            //     }
            //     else
            //     {
            //         Assert.Equal("anothertest", ev.Subject);
            //         id2Found = true;
            //     }
            // }
            //
            // Assert.True(id1Found, "id1Found");
            // Assert.True(id2Found, "id2Found");
        }

        [Fact]
        public async Task CanOverrideDefaultEventCreationOptions()
        {
            throw new NotImplementedException();
            // var serviceProvider = Init(services =>
            // {
            //     services.AddCloudEventSources();
            //     services.AddCloudEventPublisher();
            //     services.AddLocal();
            //     services.AddEventSource<TestEventSource>();
            //
            //     services.Configure<CloudEventCreationOptions>(options =>
            //     {
            //         options.Subject = "default";
            //     });
            // });
            //
            // var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            //
            // var id = await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configureDefaultCloudEventCreationOptions: options =>
            // {
            //     options.Subject = "changed";
            // });
            //
            // await eventSourceInstanceManager.StartAll();
            //
            // await Task.Delay(TimeSpan.FromSeconds(2));
            //
            // Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);
            //
            // Assert.All(MyTestCloudEventPublisher.PublishedEvents, cloudEvent =>
            // {
            //     Assert.Equal("changed", cloudEvent.Subject);
            // });
        }

        [Fact]
        public async Task EventContainsEventSourceId()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventDataflowChannels();   
                services.AddChannel("test", (provider, channelOptions) =>
                {
                    channelOptions.Endpoints.Add(new CloudEventsEndpoint(ev =>
                    {
                        MyTestCloudEventPublisher.PublishedEvents.Add(ev);

                        return Task.CompletedTask;
                    }));
                });

                services.Configure<DefaultChannelOptions>(options =>
                {
                    options.DefaultChannelName = "test";
                });
                
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            var id = await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1));

            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(2));

            var firstEvent = MyTestCloudEventPublisher.PublishedEvents.First();
            var eventSourceId = firstEvent.EventSourceId();

            Assert.NotNull(eventSourceId);
            Assert.NotEqual(string.Empty, eventSourceId);

            Assert.Equal(id, eventSourceId);
        }

        [Fact]
        public async Task EventContainsEventSourceIdWithPublisherConfiguration()
        {
            throw new NotImplementedException();
            //
            // var serviceProvider = Init(services =>
            // {
            //     services.AddCloudEventSources();
            //     services.AddCloudEventPublisher();
            //     services.AddLocal();
            //     services.AddEventSource<TestEventSource>();
            // });
            //
            // var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            //
            // var id = await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configureDefaultCloudEventCreationOptions: options =>
            // {
            //     options.Subject = "hello";
            // });
            //
            // await eventSourceInstanceManager.StartAll();
            //
            // await Task.Delay(TimeSpan.FromSeconds(2));
            //
            // var firstEvent = MyTestCloudEventPublisher.PublishedEvents.First();
            //
            // Assert.Equal("hello", firstEvent.Subject);
            // var eventSourceId = firstEvent.EventSourceId();
            //
            // Assert.NotNull(eventSourceId);
            // Assert.NotEqual(string.Empty, eventSourceId);
            //
            // Assert.Equal(id, eventSourceId);
        }

        [Fact]
        public async Task EventSourceIdsAreCorrectWhenMultipleInstancesRegistered()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            var firstSourceId = await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configure: new Action<TestEventSource>(
                source =>
                {
                    source.ExtraFile = "first.test";
                }));

            var anotherSourceId = await eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configure: new Action<TestEventSource>(
                source =>
                {
                    source.ExtraFile = "another.test";
                }));

            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(2));

            var allEvents = MyTestCloudEventPublisher.PublishedEvents.Select(CloudEvent<NewFileEvent>.Create).ToList();

            var firstFileEvent = allEvents.FirstOrDefault(x => x.Object.FileName == "first.test");
            var anotherFileEvent = allEvents.FirstOrDefault(x => x.Object.FileName == "another.test");

            Assert.Equal(firstSourceId, firstFileEvent.EventSourceId());
            Assert.Equal(anotherSourceId, anotherFileEvent.EventSourceId());
        }

        [Fact]
        public async Task DoesNotThrowIfNoEvents()
        {
            var server = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<MultiEventSource>(options =>
                {
                    options.Autostart = true;
                    options.PollingFrequency = TimeSpan.FromSeconds(1);
                });
            });

            await Task.Delay(TimeSpan.FromSeconds(3));

            await ContinueWhen(() =>
            {
                return MyTestCloudEventPublisher.PublishedEvents.Any(x => x.Type == "NewFileEvent");
            });

            await ContinueWhen(() =>
            {
                return MyTestCloudEventPublisher.PublishedEvents.Any(x => x.Type == "DeletedFileEvent");
            });
        }

        [Fact]
        public void CanGetConfigurationType()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSourceWithConfigurationTypeExtensions();
            });

            var definitionProvider = serviceProvider.GetRequiredService<IEventSourceDefinitionProvider>();
            var definitionConfigurationProvider = serviceProvider.GetRequiredService<IEventSourceDefinitionConfigurationTypeProvider>();
            var definition = definitionProvider.GetByType(typeof(EventSourceWithConfigurationType));
            var configurationType = definitionConfigurationProvider.Get(definition);

            Assert.Equal(typeof(EventSourceWithConfigurationTypeTheConfigurationType), configurationType);
        }

        [Fact]
        public void CanGetConfigurationTypeByConstructorParameterName()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<EventSourceWithConfigurationType>();
            });

            var definitionProvider = serviceProvider.GetRequiredService<IEventSourceDefinitionProvider>();
            var definitionConfigurationProvider = serviceProvider.GetRequiredService<IEventSourceDefinitionConfigurationTypeProvider>();
            var definition = definitionProvider.GetByType(typeof(EventSourceWithConfigurationType));
            var configurationType = definitionConfigurationProvider.Get(definition);

            Assert.Equal(typeof(EventSourceWithConfigurationTypeTheConfigurationType), configurationType);
        }

        [Fact]
        public async Task CanInitializeConfigurationWithDefault()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<EventSourceWithConfigurationType>();
            });

            var manager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            await manager.Create("EventSourceWithConfigurationType", pollingFrequency: TimeSpan.FromSeconds(1));
            await manager.StartAll();

            await ContinueWhen(() => MyTestCloudEventPublisher.PublishedEvents.Any());
        }

        [Fact]
        public async Task TryingToCreateEventInstanceFromUnknownEventSourceGivesClearException()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<EventSourceWithConfigurationType>();
            });

            var manager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            await Assert.ThrowsAsync<UnknownEventSourceException>(async () =>
                await manager.Create("UnknownEventSource", pollingFrequency: TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public void CanGetIfPollingIsRequired()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<EventSourceWithConfigurationType>();
            });

            var configurationTypeProvider = serviceProvider.GetRequiredService<IEventSourceDefinitionConfigurationTypeProvider>();
            var confType = configurationTypeProvider.Get("EventSourceWithConfigurationType");

            Assert.True(confType.RequiresPolling);
        }

        [Fact]
        public void CanSetInstanceId()
        {
            var id = Guid.NewGuid().ToString();

            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();

                services.AddSingleton(new EventSourceInstanceOptions()
                {
                    Autostart = false, PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "TestEventSource", Id = id
                });
            });

            var instance = serviceProvider.GetRequiredService<IEventSourceInstanceManager>().GetAll().Single();

            Assert.Equal(id, instance.Id);
        }

        [Fact]
        public async Task CanNotSetDuplicateId()
        {
            var id = Guid.NewGuid().ToString();

            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "TestEventSource", Id = id };

            await eventSourceInstanceManager.Create(options);

            await Assert.ThrowsAsync<DuplicateEventSourceInstanceException>(async () =>
            {
                await eventSourceInstanceManager.Create(options);
            });
        }
        
        [Fact]
        public async Task CanConfigureChannel()
        {
            var id = Guid.NewGuid().ToString();
            var opt = new CloudEventCreationOptions { Subject = "changed" };

            Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();

                services.AddSingleton(new EventSourceInstanceOptions()
                {
                    Autostart = true, PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "TestEventSource", Id = id,
                    ConfigureChannel = options =>
                    {
                        options.CloudEventCreationOptions = opt;
                    }
                });
            });

            await ContinueWhen(MyTestCloudEventPublisher.PublishedEvents.Any);
            var ev = MyTestCloudEventPublisher.PublishedEvents.First();
            
            Assert.Equal("changed", ev.Subject);
        }

        public void Dispose()
        {
        }
    }
}
