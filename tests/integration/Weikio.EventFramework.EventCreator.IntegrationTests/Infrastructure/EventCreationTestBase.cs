using System;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure
{
    public abstract class EventCreationTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
  
        protected EventCreationTestBase(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }
        
        protected ICloudEventCreator Init(Action<IServiceCollection> action = null)
        {
            var server = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    action?.Invoke(services);
                    
                    services.AddCloudEventCreator();
                });
                
            });

            return server.Services.GetService<ICloudEventCreator>();
            // return result;
        }
    }
}
