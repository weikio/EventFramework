using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.Dataflow.CloudEvents;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway.Http;
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
    public class HttpEventSourceTests : PollingEventSourceTestBase, IDisposable
    {
        IServiceProvider serviceProvider = null;
        private List<CloudEvent> ReceivedEvents { get; set; } = new List<CloudEvent>();

        public HttpEventSourceTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
            serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddHttpGateways();

                services.AddChannel("test", (provider, options) =>
                {
                    options.Endpoints.Add(new CloudEventsEndpoint(ev =>
                    {
                        ReceivedEvents.Add(ev);

                        return Task.CompletedTask;
                    }));
                });

                services.AddEventSource<HttpEventSource>();

                services.Configure<DefaultChannelOptions>(options =>
                {
                    options.DefaultChannelName = "test";
                });
            });

            MyTestCloudEventPublisher.PublishedEvents = new List<CloudEvent>();
        }

        [Fact]
        public async Task CanCreateHttpEventSource()
        {
            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            var options = new EventSourceInstanceOptions() { EventSourceDefinition = "Weikio.EventFramework.EventGateway.Http.HttpEventSource" };

            await eventSourceInstanceManager.Create(options);

            await eventSourceInstanceManager.StartAll();

            await Task.Delay(TimeSpan.FromSeconds(3));

            var ev = CloudEventCreator.Create(new CustomerCreatedEvent());
            var content = ev.ToHttpContent();

            await Client.PostAsync("/api/events", content);

            await ContinueWhen(ReceivedEvents.Any);

            var publishedEvent = ReceivedEvents.Single();

            Assert.Equal(ev.Type, publishedEvent.Type);
        }
    }
}
