using System;
using System.Threading.Tasks;
using EventFrameworkTestBed;
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
    public class HostedServiceEventSourceTests : EventFrameworkTestBase
    {
        private readonly TestCloudEventPublisher _testCloudEventPublisher;
        public HostedServiceEventSourceTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
            _testCloudEventPublisher = new TestCloudEventPublisher();
        }

        [Fact]
        public async Task CanAddHostedServiceAsEventSource()
        {
            // yield return 
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddSource<ContinuousTestEventBackgroundService>();
            });

            await Task.Delay(TimeSpan.FromSeconds(5));
            
            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
        }
    }
}
