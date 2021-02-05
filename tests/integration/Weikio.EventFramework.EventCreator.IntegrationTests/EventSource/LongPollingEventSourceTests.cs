using System;
using System.Linq;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
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
    public class LongPollingEventSourceTests : PollingEventSourceTestBase
    {
        private readonly TestCloudEventPublisher _testCloudEventPublisher;

        public LongPollingEventSourceTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
            _testCloudEventPublisher = new TestCloudEventPublisher();
        }

        [Fact]
        public async Task CanAddLongPollingTypeAsEventSource()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<ContinuousTestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { EventSourceDefinition = "ContinuousTestEventSource", };

            await eventSourceInstanceManager.Create(options);

            await eventSourceInstanceManager.StartAll();

            await ContinueWhen(events => events.Any());
        }
        
        [Fact]
        public async Task CanStopLongPolling()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<ContinuousTestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { EventSourceDefinition = "ContinuousTestEventSource", };

            await eventSourceInstanceManager.Create(options);

            await eventSourceInstanceManager.StartAll();

            await ContinueWhen(events => events.Any());
            await eventSourceInstanceManager.StopAll();
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            MyTestCloudEventPublisher.PublishedEvents.Clear();
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            Assert.Empty(MyTestCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task CanUseConfiguration()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<ContinuousTestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            var options = new EventSourceInstanceOptions()
            {
                EventSourceDefinition = "ContinuousTestEventSource",
                Configuration = new ContinuousTestEventSourceConfiguration() { ExtraFile = "myextra.txt" }
            };

            await eventSourceInstanceManager.Create(options);
            await eventSourceInstanceManager.StartAll();

            await ContinueWhen(events =>
            {
                var found = events.Select(CloudEvent<NewFileEvent>.Create).FirstOrDefault(x => x.Object.FileName == "myextra.txt");

                if (found != null)
                {
                    return true;
                }

                return false;
            });
        }

        [Fact]
        public async Task LongPollingDoesNotAutostartByDefault()
        {
            var serviceProvider = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<ContinuousTestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { EventSourceDefinition = "ContinuousTestEventSource", };

            await eventSourceInstanceManager.Create(options);

            await Task.Delay(TimeSpan.FromSeconds(3));

            Assert.Empty(_testCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task CanAutoStart()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<ContinuousTestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { EventSourceDefinition = "ContinuousTestEventSource", Autostart = true };

            await eventSourceInstanceManager.Create(options);

            await ContinueWhen(events => events.Any());
        }

        [Fact]
        public async Task EachLongPollerCanHaveDifferentConfiguration()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<ContinuousTestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            var options = new EventSourceInstanceOptions()
            {
                EventSourceDefinition = "ContinuousTestEventSource",
                Configuration = new ContinuousTestEventSourceConfiguration() { ExtraFile = "first.test" }
            };
            await eventSourceInstanceManager.Create(options);

            var options2 = new EventSourceInstanceOptions()
            {
                EventSourceDefinition = "ContinuousTestEventSource",
                Configuration = new ContinuousTestEventSourceConfiguration() { ExtraFile = "second.test" }
            };
            await eventSourceInstanceManager.Create(options2);

            await eventSourceInstanceManager.StartAll();

            await ContinueWhen(events =>
            {
                var allEvents = events.Select(CloudEvent<NewFileEvent>.Create).ToList();
                var firstFileEvent = allEvents.FirstOrDefault(x => x.Object.FileName == "first.test");
                var anotherFileEvent = allEvents.FirstOrDefault(x => x.Object.FileName == "second.test");

                return firstFileEvent != null && anotherFileEvent != null;
            });
        }
        
        [Fact]
        public async Task CanRunMultiPoller()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<MultiTestLongPollerEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { EventSourceDefinition = "MultiTestLongPollerEventSource", Autostart = true };

            await eventSourceInstanceManager.Create(options);

            await ContinueWhen(events =>
            {
                var allEvents = events.Select(CloudEvent<NewFileEvent>.Create).ToList();
                var firstFileEvent = allEvents.FirstOrDefault(x => x.Object.FileName == "first.txt");
                var anotherFileEvent = allEvents.FirstOrDefault(x => x.Object.FileName == "second.txt");

                return firstFileEvent != null && anotherFileEvent != null;
            });
        }
        
        [Fact]
        public async Task CanStopMultiPoller()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<MultiTestLongPollerEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { EventSourceDefinition = "MultiTestLongPollerEventSource", Autostart = true };

            await eventSourceInstanceManager.Create(options);

            await ContinueWhen(events =>
            {
                var allEvents = events.Select(CloudEvent<NewFileEvent>.Create).ToList();
                var firstFileEvent = allEvents.FirstOrDefault(x => x.Object.FileName == "first.txt");
                var anotherFileEvent = allEvents.FirstOrDefault(x => x.Object.FileName == "second.txt");

                return firstFileEvent != null && anotherFileEvent != null;
            });

            await eventSourceInstanceManager.StopAll();
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            MyTestCloudEventPublisher.PublishedEvents.Clear();
            
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            Assert.Empty(MyTestCloudEventPublisher.PublishedEvents);
        }
    }
}
