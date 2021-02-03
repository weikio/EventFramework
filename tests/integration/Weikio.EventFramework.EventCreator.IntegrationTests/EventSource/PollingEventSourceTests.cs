using System;
using System.Linq;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.EventCreator.IntegrationTests.EventSource.Sources;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.EventSourceWrapping;
using Weikio.EventFramework.EventSource.Polling;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.EventSource
{
    public class PollingEventSourceTests : PollingEventSourceTestBase, IDisposable
    {
        private readonly TestCloudEventPublisher _testCloudEventPublisher;
        private int _counter;

        public PollingEventSourceTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
            _testCloudEventPublisher = new TestCloudEventPublisher();
            _counter = 0;
        }

        [Fact]
        public async Task CanAddEventReturningTypeAsEventSource()
        {
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddSource<TestEventSource>(pollingFrequency: TimeSpan.FromSeconds(1));
            });

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task CanConfigurePollingFrequencyUsingOptions()
        {
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.Configure<PollingOptions>(options =>
                {
                    options.PollingFrequency = TimeSpan.FromSeconds(100);
                });
                services.AddSource<TestEventSource>();
            });

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.Empty(_testCloudEventPublisher.PublishedEvents);
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
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

             eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configure: new Action<TestEventSource>(source =>
            {
                source.ExtraFile = "first.test";
            }));

            eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(10), configure: new Action<TestEventSource>(source =>
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
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configure: new Action<TestEventSource>(source =>
            {
                source.ExtraFile = "first.test";
            }));

            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(2));

            var instanceFile = _testCloudEventPublisher.PublishedEvents.OfType<NewFileEvent>().FirstOrDefault(x => x.FileName == "first.test");
            Assert.NotNull(instanceFile);
        }
        
        [Fact]
        public async Task CanConfigureEventCreationOptions()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            var id  = eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configurePublisherOptions: options =>
            {
                options.ConfigureDefaultCloudEventCreationOptions = creationOptions =>
                {
                    creationOptions.Subject = "mytest";
                };
            });
            
            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);

            foreach (var ev in MyTestCloudEventPublisher.PublishedEvents)     
            {
                Assert.Equal("mytest", ev.Subject);
            }
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
            eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1));

            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(2));
            
            Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);

            foreach (var ev in MyTestCloudEventPublisher.PublishedEvents)     
            {
                Assert.Equal("another", ev.Subject);
            }
        }
        
        [Fact]
        public async Task CanConfigureInstanceSpecificCreationOptions()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var id  = eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configurePublisherOptions: options =>
            {
                options.ConfigureDefaultCloudEventCreationOptions = creationOptions =>
                {
                    creationOptions.Subject = "mytest";
                };
            });
            
            var id2  = eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configurePublisherOptions: options =>
            {
                options.ConfigureDefaultCloudEventCreationOptions = creationOptions =>
                {
                    creationOptions.Subject = "anothertest";
                };
            });
            
            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);

            var id1Found = false;
            var id2Found = false;
            foreach (var ev in MyTestCloudEventPublisher.PublishedEvents)     
            {
                if (ev.EventSourceId() == id)
                {
                    Assert.Equal("mytest", ev.Subject);
                    id1Found = true;
                }
                else
                {
                    Assert.Equal("anothertest", ev.Subject);
                    id2Found = true;
                }
            }
            
            Assert.True(id1Found, "id1Found");
            Assert.True(id2Found, "id2Found");
        }
        
        [Fact]
        public async Task CanOverrideDefaultEventCreationOptions()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
                services.Configure<CloudEventCreationOptions>(options =>
                {
                    options.Subject = "default";
                });
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var id  = eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configurePublisherOptions: options =>
            {
                options.ConfigureDefaultCloudEventCreationOptions = creationOptions =>
                {
                    creationOptions.Subject = "changed";
                };
            });

            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(2));
            
            Assert.NotEmpty(MyTestCloudEventPublisher.PublishedEvents);
            Assert.All(MyTestCloudEventPublisher.PublishedEvents, cloudEvent =>
            {
                Assert.Equal("changed", cloudEvent.Subject);
            });
        }

        [Fact]
        public async Task EventContainsEventSourceId()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            var id  = eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1));
            
            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(2));

            var firstEvent = MyTestCloudEventPublisher.PublishedEvents.First();
            var eventSourceId = firstEvent.EventSourceId();

            Assert.NotNull(eventSourceId);
            Assert.NotEqual(Guid.Empty, eventSourceId);
            
            Assert.Equal(id, eventSourceId);
        }
        
        [Fact]
        public async Task EventContainsEventSourceIdWithPublisherConfiguration()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            var id  = eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configurePublisherOptions: options =>
            {
                options.ConfigureDefaultCloudEventCreationOptions = creationOptions =>
                {
                    creationOptions.Subject = "hello";
                };
            });
            
            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(2));

            var firstEvent = MyTestCloudEventPublisher.PublishedEvents.First();
            
            Assert.Equal("hello", firstEvent.Subject);
            var eventSourceId = firstEvent.EventSourceId();

            Assert.NotNull(eventSourceId);
            Assert.NotEqual(Guid.Empty, eventSourceId);
            
            Assert.Equal(id, eventSourceId);
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

            var firstSourceId = eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configure: new Action<TestEventSource>(source =>
            {
                source.ExtraFile = "first.test";
            }));

            var anotherSourceId = eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configure: new Action<TestEventSource>(source =>
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
        public async Task CanAddEventReturningTypeWithMultipleMethodsAsEventSource()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task CanAddPublishingTypeAsEventSource()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task CanAddPublishingTypeWithStateAsEventSource()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task CanAddStatelessEventSource()
        {
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddSource(() =>
                {
                    return new CustomerCreatedEvent();
                }, TimeSpan.FromSeconds(1));
            });

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task CanAddStatelessEventSourceInstance()
        {
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                var testEventSource = new TestEventSource("instance.test");
                services.AddSource(testEventSource, TimeSpan.FromSeconds(1));
            });

            await Task.Delay(TimeSpan.FromSeconds(2));

            var instanceFile = _testCloudEventPublisher.PublishedEvents.OfType<NewFileEvent>().FirstOrDefault(x => x.FileName == "instance.test");
            Assert.NotNull(instanceFile);
        }

        [Fact]
        public async Task StatelessEventSourceInstanceDoesNotThrowIfNoNewEventsAreAvailable()
        {
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                var testEventSource = new TestEventSource("instance.test");
                services.AddSource(testEventSource, TimeSpan.FromSeconds(1));
            });

            await Task.Delay(TimeSpan.FromSeconds(3));

            var instanceFile = _testCloudEventPublisher.PublishedEvents.OfType<NewFileEvent>().FirstOrDefault(x => x.FileName == "instance.test");
            Assert.NotNull(instanceFile);
        }

        [Fact]
        public async Task StatelessEventSourceIsNotRunInStart()
        {
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddSource(() => new CustomerCreatedEvent(), TimeSpan.FromMinutes(3));
            });

            await Task.Delay(TimeSpan.FromSeconds(3));

            Assert.Empty(_testCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task StatefullEventSourceIsInitializedInStart()
        {
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddSource<int>(currentCount =>
                {
                    _counter = currentCount + 10;

                    return (new CustomerCreatedEvent(), _counter);
                }, TimeSpan.FromMinutes(5));
            });

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.Equal(10, _counter);

            // Events aren't published yet as this is just an initialization run
            Assert.Empty(_testCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task StatefullEventSourceWithCronIntervalIsInitializedInStart()
        {
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddSource<int>(currentCount =>
                {
                    _counter = currentCount + 10;

                    return (new CustomerCreatedEvent(), _counter);
                }, cronExpression: "0 0 12 * * ?");
            });

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.Equal(10, _counter);

            // Events aren't published yet as this is just an initialization run
            Assert.Empty(_testCloudEventPublisher.PublishedEvents);
        }

        public void Dispose()
        {
        }
    }
}
