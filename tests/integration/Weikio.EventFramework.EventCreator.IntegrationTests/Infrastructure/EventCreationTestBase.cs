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
    public abstract class EventCreationTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        protected Func<object> ObjectFactory;
        protected Func<List<object>> MultiObjectFactory;
  
        protected EventCreationTestBase(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }
        
        protected HttpClient Init(Action<IServiceCollection> action = null, Action<ObjectFactoryOptions> objectSetupAction = null)
        {
            var result = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    action?.Invoke(services);

                    if (objectSetupAction != null)
                    {
                        services.Configure(objectSetupAction);
                    }
                    else
                    {
                        var opt = new ObjectFactoryOptions { Create = ObjectFactory, CreateMulti = MultiObjectFactory};
                        var optionsConfigure = Options.Create(opt);

                        services.AddSingleton(optionsConfigure);
                    }
                    
                    services.AddCloudEventCreator();
                });
                
            }).CreateClient();

            return result;
        }
    }
}
