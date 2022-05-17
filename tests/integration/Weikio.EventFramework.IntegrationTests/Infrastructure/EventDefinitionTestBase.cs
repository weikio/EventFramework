using System;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.EventDefinition;
using Xunit;

namespace Weikio.EventFramework.IntegrationTests.Infrastructure
{
    public abstract class EventDefinitionTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
  
        protected EventDefinitionTestBase(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }
        
        protected IServiceProvider Init(Action<IServiceCollection> action = null)
        {
            var server = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    action?.Invoke(services);
                    
                    services.AddCloudEventDefinitions();
                });
                
            });

            return server.Services;
        }
    }
}
