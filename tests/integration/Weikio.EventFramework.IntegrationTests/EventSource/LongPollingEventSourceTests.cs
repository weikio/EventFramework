using System;
using System.Linq;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.EventSourceWrapping;
using Weikio.EventFramework.IntegrationTests.EventSource.Sources;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.EventSource
{
    [Collection(nameof(NotThreadSafeResourceCollection))]
    public class LongPollingEventSourceTests : PollingEventSourceTestBase
    {

        public LongPollingEventSourceTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
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
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<ContinuousTestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { EventSourceDefinition = "ContinuousTestEventSource", };

            await eventSourceInstanceManager.Create(options);

            await Task.Delay(TimeSpan.FromSeconds(3));

            Assert.Empty(MyTestCloudEventPublisher.PublishedEvents);
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
        
        [Fact]
        public async Task TestStatusLifecycle()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<ContinuousTestEventSource>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "ContinuousTestEventSource" };

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
    }
}
