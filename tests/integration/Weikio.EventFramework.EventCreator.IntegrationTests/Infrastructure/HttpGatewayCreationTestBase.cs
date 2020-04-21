using System;
using System.Collections.Generic;
using System.Net.Http;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Creator;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventCreator;
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
