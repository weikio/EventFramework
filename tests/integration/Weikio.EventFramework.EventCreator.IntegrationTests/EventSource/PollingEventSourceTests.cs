using System;
using System.Linq;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.EventCreator.IntegrationTests.EventSource.Sources;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.EventSource
{
    public class PollingEventSourceTests : EventFrameworkTestBase, IDisposable
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
