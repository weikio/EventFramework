using System;
using System.Linq;
using System.Threading.Tasks;
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
            });

            var factory = serviceProvider.GetRequiredService<IEventSourceFactory>();
            var eventSource = factory.Create<TestEventSource>(TimeSpan.FromSeconds(1));

            var manager = serviceProvider.GetRequiredService<IEventSourceManager>();
            manager.Add(eventSource);
            
            manager.Update();
            
            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
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
}
