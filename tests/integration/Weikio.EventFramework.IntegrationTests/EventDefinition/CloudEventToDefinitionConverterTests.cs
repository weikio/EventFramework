using System;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventDefinition;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;

namespace Weikio.EventFramework.IntegrationTests.EventDefinition
{
    public class CloudEventToDefinitionConverterTests : EventDefinitionTestBase
    {
        public CloudEventToDefinitionConverterTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
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

            var manager = provider.GetRequiredService<CloudEventToDefinitionConverter>();

            var result = await manager.Convert(ev);
            throw new NotImplementedException();
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

            var manager = provider.GetRequiredService<CloudEventToDefinitionConverter>();

            var result = await manager.Convert(ev);

            throw new NotImplementedException();
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
            var json = JsonConvert.SerializeObject(ev.Data);
            var token = JToken.Parse(json);
            ev.Data = token;

            var manager = provider.GetRequiredService<CloudEventToDefinitionConverter>();

            var result = await manager.Convert(ev);

            throw new NotImplementedException();
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

            var manager = provider.GetRequiredService<CloudEventToDefinitionConverter>();

            var result = await manager.Convert(ev);

            throw new NotImplementedException();
        }
    }
}
