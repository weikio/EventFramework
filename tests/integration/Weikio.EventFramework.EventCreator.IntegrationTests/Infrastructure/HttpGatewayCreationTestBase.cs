using System;
using System.Net.Http;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure
{
    public abstract class EventFrameworkTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
  
        protected EventFrameworkTestBase(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }
        
        protected HttpClient Init(Action<IServiceCollection> action = null)
        {
            var result = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    action?.Invoke(services);
                });
                
            }).CreateClient();

            return result;
        }
    }
}
