using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Extensions;
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
    public class HostedServiceEventSourceTests : PollingEventSourceTestBase, IDisposable
    {
        IServiceProvider serviceProvider = null;
        
        public HostedServiceEventSourceTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
            serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<ContinuousTestEventBackgroundService>();
            });
            
            MyTestCloudEventPublisher.PublishedEvents = new List<CloudEvent>();
        }

        [Fact]
        public async Task CanStart()
        {
            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { EventSourceDefinition = "ContinuousTestEventBackgroundService", };

            await eventSourceInstanceManager.Create(options);

            await eventSourceInstanceManager.StartAll();

            await ContinueWhen(MyTestCloudEventPublisher.PublishedEvents.Any);
        }
        
        [Fact]
        public async Task CanStop()
        {
            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { EventSourceDefinition = "ContinuousTestEventBackgroundService", };

            await eventSourceInstanceManager.Create(options);

            await eventSourceInstanceManager.StartAll();
            await ContinueWhen(events => events.Any());

            await eventSourceInstanceManager.StopAll();
            await Task.Delay(TimeSpan.FromSeconds(1));
            MyTestCloudEventPublisher.PublishedEvents.Clear();
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            Assert.Empty(MyTestCloudEventPublisher.PublishedEvents);
        }
        
        [Fact]
        public async Task EventContainsEventSourceId()
        {
            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            var id = await eventSourceInstanceManager.Create("ContinuousTestEventBackgroundService");

            await eventSourceInstanceManager.StartAll();

            await ContinueWhen(MyTestCloudEventPublisher.PublishedEvents.Any);
            
            var firstEvent = MyTestCloudEventPublisher.PublishedEvents.First();
            var eventSourceId = firstEvent.EventSourceId();

            Assert.NotNull(eventSourceId);
            Assert.NotEqual(Guid.Empty, eventSourceId);

            Assert.Equal(id, eventSourceId);
        }
        
        [Fact]
        public async Task CanUseConfiguration()
        {
            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            var options = new EventSourceInstanceOptions()
            {
                EventSourceDefinition = "ContinuousTestEventBackgroundService",
                Configuration = new ContinuousTestEventSourceConfiguration() { ExtraFile = "myextra.txt" }
            };

            await eventSourceInstanceManager.Create(options);
            await eventSourceInstanceManager.StartAll();

            await ContinueWhen(events =>
            {
                var found = events.Select(CloudEvent<string>.Create).FirstOrDefault(x => string.Equals(x.Object, "myextra.txt"));

                if (found != null)
                {
                    return true;
                }

                return false;
            }, "No extra event found");
        }
        
        [Fact]
        public async Task CanAutoStart()
        {
            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();

            var options = new EventSourceInstanceOptions()
            {
                EventSourceDefinition = "ContinuousTestEventBackgroundService",
                Autostart = true
            };

            await eventSourceInstanceManager.Create(options);

            await ContinueWhen(MyTestCloudEventPublisher.PublishedEvents.Any);
        }
        
                
        [Fact]
        public async Task TestStatusLifecycle()
        {
            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions()
            {
                EventSourceDefinition = "ContinuousTestEventBackgroundService"
            };

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
