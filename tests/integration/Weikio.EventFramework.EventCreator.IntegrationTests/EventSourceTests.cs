using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Xunit;

namespace Weikio.EventFramework.EventCreator.IntegrationTests
{
    public class EventSourceTests : EventFrameworkTestBase
    {
        private TestCloudEventPublisher _testCloudEventPublisher;
        private int _counter;
        public EventSourceTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
            _testCloudEventPublisher = new TestCloudEventPublisher();
            _counter = 0;
        }

        [Fact]
        public async Task CanAddHostedServiceAsEventSource()
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
                
                services.AddSource(() => new CustomerCreatedEvent(), TimeSpan.FromSeconds(1));
            });

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
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

            await Task.Delay(TimeSpan.FromSeconds(2));

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
    }

    public class CounterUpdatedEvent
    {
        public int Count { get; }

        public CounterUpdatedEvent(int count)
        {
            Count = count;
        }
    }
    
    public class TestCloudEventPublisher : ICloudEventPublisher
    {
        public List<object> PublishedEvents = new List<object>();

        public TestCloudEventPublisher()
        {
        }

        public Task<CloudEvent> Publish(CloudEvent cloudEvent, string gatewayName = GatewayName.Default)
        {
            PublishedEvents.Add(cloudEvent);
            return Task.FromResult<CloudEvent>(cloudEvent);
        }

        public Task<List<CloudEvent>> Publish(IList<object> objects, string eventTypeName = "", string id = "", Uri source = null, string gatewayName = GatewayName.Default)
        {
            PublishedEvents.AddRange(objects);

            return Task.FromResult(new List<CloudEvent>());
        }

        public Task<CloudEvent> Publish(object obj, string eventTypeName = "", string id = "", Uri source = null, string gatewayName = GatewayName.Default)
        {
            PublishedEvents.Add(obj);

            return Task.FromResult(new CloudEvent("test", new Uri("http://localhost", UriKind.Absolute), Guid.NewGuid().ToString()));
        }
    }
}
