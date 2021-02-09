using System;
using System.Linq;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
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
    public class HostedServiceEventSourceTests : PollingEventSourceTestBase
    {
        public HostedServiceEventSourceTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task CanStart()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<ContinuousTestEventBackgroundService>();
            });

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { EventSourceDefinition = "ContinuousTestEventBackgroundService", };

            await eventSourceInstanceManager.Create(options);

            await eventSourceInstanceManager.StartAll();

            await ContinueWhen(MyTestCloudEventPublisher.PublishedEvents.Any);
        }
        
        [Fact]
        public async Task CanStop()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddEventSource<ContinuousTestEventBackgroundService>();
            });

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
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
                services.AddEventSource<ContinuousTestEventBackgroundService>();
            });

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
    }
}
