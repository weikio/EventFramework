using System;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;

namespace Weikio.EventFramework.IntegrationTests.EventCreation
{
    public class EventDefinitionTests : EventDefinitionTestBase
    {
        public EventDefinitionTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public void CanRegisterEventDefinition()
        {
            var provider = Init(services =>
            {
                services.AddSingleton(new CloudEventDefinition()
                {
                    Source = "test",
                    Type = "another",
                });
            });

            var manager = provider.GetRequiredService<ICloudEventDefinitionManager>();
            Assert.Single(manager.List());
        }
        
        [Fact]
        public async Task CanRegisterEventDefinitionFromObjectData()
        {
            var provider = Init(services =>
            {
            });

            var creator = provider.GetRequiredService<ICloudEventCreator>();
            
            var customerCreatedEvent = new CustomerCreatedEvent();
            var ev = creator.CreateCloudEvent(customerCreatedEvent);
            
            var manager = provider.GetRequiredService<ICloudEventDefinitionManager>();
            
            await manager.Register(ev);
            Assert.Single(manager.List());
        }
        
        [Fact]
        public async Task CanRegisterEventDefinitionFromJsonData()
        {
            var provider = Init(services =>
            {
            });

            var creator = provider.GetRequiredService<ICloudEventCreator>();
            
            var customerCreatedEvent = new CustomerCreatedEvent();
            var ev = creator.CreateCloudEvent(customerCreatedEvent);
            ev.Data = JsonConvert.SerializeObject(ev.Data);
            
            var manager = provider.GetRequiredService<ICloudEventDefinitionManager>();
            
            await manager.Register(ev);
            Assert.Single(manager.List());
        }
        
        [Fact]
        public async Task CanRegisterEventDefinitionFromJTokenData()
        {
            var provider = Init(services =>
            {
            });

            var creator = provider.GetRequiredService<ICloudEventCreator>();
            
            var customerCreatedEvent = new CustomerCreatedEvent();
            var ev = creator.CreateCloudEvent(customerCreatedEvent);
            var json =JsonConvert.SerializeObject(ev.Data);
            var token = JToken.Parse(json);
            ev.Data = token;
            
            var manager = provider.GetRequiredService<ICloudEventDefinitionManager>();
            
            await manager.Register(ev);
            Assert.Single(manager.List());
        }
        
        [Fact]
        public async Task CanRegisterEventDefinitionFromUrl()
        {
            var provider = Init(services =>
            {
            });

            var creator = provider.GetRequiredService<ICloudEventCreator>();
            
            var customerCreatedEvent = new CustomerCreatedEvent();
            var ev = creator.CreateCloudEvent(customerCreatedEvent);
            ev.DataSchema = new Uri("https://raw.githubusercontent.com/linux-china/cloud-events-java-api/master/cloud-events-schema.json");
            
            var manager = provider.GetRequiredService<ICloudEventDefinitionManager>();
            
            await manager.Register(ev);
            Assert.Single(manager.List());
        }

    }
}
