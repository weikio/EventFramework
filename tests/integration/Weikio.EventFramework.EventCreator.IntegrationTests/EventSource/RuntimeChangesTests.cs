using System;
using System.Threading.Tasks;
using EventFrameworkTestBed;
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
        private int _counter;

        public RuntimeChangesTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
            _testCloudEventPublisher = new TestCloudEventPublisher();
            _counter = 0;
        }
        
        [Fact]
        public async Task CanAddPollingEventSourceAfterApplicationHasStarted()
        {
            var provider = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();
            });

            var factory = provider.GetRequiredService<EventSourceFactory>();
            var eventSource = factory.Create<TestEventSource>(TimeSpan.FromSeconds(1));

            var manager = provider.GetRequiredService<EventSourceManager>();
            manager.Add(eventSource);
            
            manager.Update();
            
            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
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
