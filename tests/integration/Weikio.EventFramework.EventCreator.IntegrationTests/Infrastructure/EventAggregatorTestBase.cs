using System;
using System.Net.Http;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.EventAggregator.Core;
using Weikio.EventFramework.EventPublisher;
using Xunit;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure
{
    public abstract class EventAggregatorTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        protected EventAggregatorTestBase(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        protected EventAggregatorPublisherForTesting Init(Action<IServiceCollection> action = null)
        {
            var server = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddCloudEventAggregatorCore();
                    services.AddCloudEventCreator();
                    action?.Invoke(services);

                    services.AddSingleton<EventAggregatorPublisherForTesting>();
                });
            });

            return server.Services.GetService<EventAggregatorPublisherForTesting>();
        }
    }
}
