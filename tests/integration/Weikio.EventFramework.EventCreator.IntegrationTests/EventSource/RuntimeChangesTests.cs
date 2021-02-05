using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    public class RuntimeChangesTests : RuntimeChangesTestBase,  IDisposable
    {
        private readonly TestCloudEventPublisher _testCloudEventPublisher;

        public RuntimeChangesTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
            _testCloudEventPublisher = new TestCloudEventPublisher();
        }
        
        [Fact]
        public async Task CanAddPollingEventSourceRuntime()
        {
            var serviceProvider = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceProvider = serviceProvider.GetRequiredService<EventSourceProvider>();
            var eventSource = eventSourceProvider.Get("TestEventSource");

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var esInstanceGuid = await eventSourceInstanceManager.Create(eventSource, TimeSpan.FromSeconds(1));

            await eventSourceInstanceManager.Start(esInstanceGuid);
            
            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
        }
        
        [Fact]
        public async Task CanAddMultiplePollingEventSourcesRuntime()
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
            
            // Add first
            eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configure: new Action<TestEventSource>(source =>
            {
                source.ExtraFile = "first.test";
            }));;

            // Add second with configuration
            eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1), configure: new Action<TestEventSource>(source =>
            {
                source.ExtraFile = "second.test";
            }));

            await eventSourceInstanceManager.StartAll();
            
            await Task.Delay(TimeSpan.FromSeconds(2));

            var instanceFile = _testCloudEventPublisher.PublishedEvents.OfType<NewFileEvent>().FirstOrDefault(x => x.FileName == "first.test");
            Assert.NotNull(instanceFile);
            
            var anotherFile = _testCloudEventPublisher.PublishedEvents.OfType<NewFileEvent>().FirstOrDefault(x => x.FileName == "second.test");
            Assert.NotNull(anotherFile);
        }
        
        [Fact]
        public async Task CanStop()
        {
            var serviceProvider = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceProvider = serviceProvider.GetRequiredService<EventSourceProvider>();
            var eventSource = eventSourceProvider.Get("TestEventSource");

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            eventSourceInstanceManager.Create(eventSource, TimeSpan.FromSeconds(1));
            await eventSourceInstanceManager.StartAll();
            await Task.Delay(TimeSpan.FromSeconds(2));

            await eventSourceInstanceManager.StartAll();
            await eventSourceInstanceManager.StopAll();
            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
            
            _testCloudEventPublisher.PublishedEvents.Clear();
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            Assert.Empty(_testCloudEventPublisher.PublishedEvents);
        }
        
        [Fact]
        public async Task CanStopAndStart()
        {
            var serviceProvider = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceProvider = serviceProvider.GetRequiredService<EventSourceProvider>();
            var eventSource = eventSourceProvider.Get("TestEventSource");

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            eventSourceInstanceManager.Create(eventSource, TimeSpan.FromSeconds(1));
            await eventSourceInstanceManager.StartAll();
            await Task.Delay(TimeSpan.FromSeconds(2));

            await eventSourceInstanceManager.StartAll();
            await eventSourceInstanceManager.StopAll();
            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
            
            _testCloudEventPublisher.PublishedEvents.Clear();
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            Assert.Empty(_testCloudEventPublisher.PublishedEvents);
            await eventSourceInstanceManager.StartAll();
            await Task.Delay(TimeSpan.FromSeconds(2));
            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
        }
        
        [Fact]
        public async Task CanConfigurePublishedEvent()
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
            eventSourceInstanceManager.Create("TestEventSource", TimeSpan.FromSeconds(1));
            
            
        }
        
        [Fact]
        public async Task CanStopPollingEventSource()
        {
            var serviceProvider = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<TestEventSource>();
            });

            var eventSourceProvider = serviceProvider.GetRequiredService<EventSourceProvider>();
            var eventSource = eventSourceProvider.Get("TestEventSource");

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            eventSourceInstanceManager.Create(eventSource, TimeSpan.FromSeconds(1));

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.Empty(_testCloudEventPublisher.PublishedEvents);
        }
        
        [Fact]
        public async Task CanAddLongPollingEventSourceRuntime()
        {
            var serviceProvider = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
            });

            var factory = serviceProvider.GetRequiredService<IEventSourceFactory>();
            var eventSource = factory.Create<ContinuousTestEventSource>();

            var manager = serviceProvider.GetRequiredService<IEventSourceManager>();
            manager.Add(eventSource);
            
            manager.Update();
            
            await Task.Delay(TimeSpan.FromSeconds(5));

            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
        }
        
        [Fact]
        public async Task CanStopLongPollingEventSource()
        {
            var serviceProvider = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
            });

            var factory = serviceProvider.GetRequiredService<IEventSourceFactory>();
            var eventSource = factory.Create<ContinuousTestEventSource>();

            var manager = serviceProvider.GetRequiredService<IEventSourceManager>();
            manager.Add(eventSource);
            
            manager.Update();
            
            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.Equal(EventSourceStatusEnum.Started, eventSource.Status.Status);
            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);

            manager.Stop(eventSource.Id);
            _testCloudEventPublisher.PublishedEvents.Clear();

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.Equal(EventSourceStatusEnum.Stopped, eventSource.Status.Status);
            Assert.Empty(_testCloudEventPublisher.PublishedEvents);
        }

        
        [Fact]
        public async Task CanAddHostedServiceEventSourceRuntime()
        {
            var serviceProvider = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
            });

            var factory = serviceProvider.GetRequiredService<IEventSourceFactory>();
            var eventSource = factory.Create<ContinuousTestEventBackgroundService>();

            var manager = serviceProvider.GetRequiredService<IEventSourceManager>();
            manager.Add(eventSource);
            
            manager.Update();
            
            await Task.Delay(TimeSpan.FromSeconds(5));

            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
        }
        
        [Fact]
        public async Task CanAddStatelessEventSourceRuntime()
        {
            var serviceProvider = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
            });

            var factory = serviceProvider.GetRequiredService<IEventSourceFactory>();
            var eventSource = factory.Create(() => new CustomerCreatedEvent(), TimeSpan.FromSeconds(1));

            var manager = serviceProvider.GetRequiredService<IEventSourceManager>();
            manager.Add(eventSource);
            
            manager.Update();
            
            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
        }
        
        [Fact]
        public async Task CanAddStatelessEventSourceInstanceRuntime()
        {
            var serviceProvider = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
            });

            var factory = serviceProvider.GetRequiredService<IEventSourceFactory>();
            var testEventSource = new TestEventSource("instance.test");
            var eventSource = factory.Create(testEventSource, TimeSpan.FromSeconds(1));

            var manager = serviceProvider.GetRequiredService<IEventSourceManager>();
            manager.Add(eventSource);
            
            manager.Update();
            
            await Task.Delay(TimeSpan.FromSeconds(2));

            var instanceFile = _testCloudEventPublisher.PublishedEvents.OfType<NewFileEvent>().FirstOrDefault(x => x.FileName == "instance.test");
            Assert.NotNull(instanceFile);
        }
        
        [Fact]
        public async Task CanAddMultipleEventSourceInstancesRuntime()
        {
            var serviceProvider = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
            });

            var factory = serviceProvider.GetRequiredService<IEventSourceFactory>();
            
            var testEventSource = new TestEventSource("instance.test");
            var anotherTestEventSource = new TestEventSource("another.test");
            var eventSource1 = factory.Create(testEventSource, TimeSpan.FromSeconds(1));
            var eventSource2 = factory.Create(anotherTestEventSource, TimeSpan.FromSeconds(1));

            var manager = serviceProvider.GetRequiredService<IEventSourceManager>();
            manager.Add(eventSource1);
            manager.Add(eventSource2);

            manager.Update();
            
            await Task.Delay(TimeSpan.FromSeconds(3));

            var instanceFile = _testCloudEventPublisher.PublishedEvents.OfType<NewFileEvent>().FirstOrDefault(x => x.FileName == "instance.test");
            Assert.NotNull(instanceFile);
            
            var anotherFile = _testCloudEventPublisher.PublishedEvents.OfType<NewFileEvent>().FirstOrDefault(x => x.FileName == "another.test");
            Assert.NotNull(anotherFile);
        }

        public void Dispose()
        {
        }
    }
    
    public abstract class RuntimeChangesTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        public ITestOutputHelper Output { get; set; }

        protected RuntimeChangesTestBase(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            Output = output;
            _factory = factory;
        }
        
        protected IServiceProvider Init(Action<IServiceCollection> action = null)
        {
            var server = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    action?.Invoke(services);
                    
                    services.AddCloudEventCreator();
                });
                
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // Remove other loggers
                    logging.AddXUnit(Output); // Use the ITestOutputHelper instance
                });
                
            });

            return server.Services;
        }
    }
    
    public abstract class PollingEventSourceTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        public ITestOutputHelper Output { get; set; }

        protected PollingEventSourceTestBase(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            Output = output;
            _factory = factory;
        }
        
        protected IServiceProvider Init(Action<IServiceCollection> action = null)
        {
            var server = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    action?.Invoke(services);
                    
                    services.AddCloudEventCreator();
                    services.AddTransient<ICloudCloudEventPublisherBuilder, TestCloudEventPublisherBuilder>();
                });
                
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // Remove other loggers
                    logging.AddXUnit(Output); // Use the ITestOutputHelper instance
                });
                
            });

            return server.Services;
        }
        
        
        public async Task ContinueWhen(Predicate<List<CloudEvent>> probe, string assertErrorMessage = null, TimeSpan? timeout = null)
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
                success = probe(MyTestCloudEventPublisher.PublishedEvents);

                if (success)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50), cts.Token);
            }

            if (success)
            {
                return;
            }

            throw new Exception(assertErrorMessage ?? "Assertion failed");
        }
        
        public async Task ContinueWhen(Func<bool> probe, string assertErrorMessage = null, TimeSpan? timeout = null)
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

                await Task.Delay(TimeSpan.FromMilliseconds(50), cts.Token);
            }

            if (success)
            {
                return;
            }

            throw new Exception(assertErrorMessage ?? "Assertion failed");
        }
    }
}
