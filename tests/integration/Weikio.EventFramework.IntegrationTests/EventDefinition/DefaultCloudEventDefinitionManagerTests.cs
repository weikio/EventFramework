using System;
using System.Linq;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventDefinition;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;

namespace Weikio.EventFramework.IntegrationTests.EventDefinition
{
    public class DefaultCloudEventDefinitionManagerTests : EventDefinitionTestBase
    {
        public DefaultCloudEventDefinitionManagerTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public void CanRegisterEventDefinitionOnStartup()
        {
            var provider = Init(services =>
            {
                services.AddSingleton(new CloudEventDefinition("test", "another"));
            });

            var manager = provider.GetRequiredService<ICloudEventDefinitionManager>();
            Assert.Single(manager.List());
        }
        
        [Fact]
        public void CanRegisterEventDefinitionRuntime()
        {
            var provider = Init(services =>
            {
            });

            var manager = provider.GetRequiredService<ICloudEventDefinitionManager>();
            manager.TryAdd(new CloudEventDefinition("test", "another"));
            
            Assert.Single(manager.List());
        }
        
        [Fact]
        public void DuplicateIsNotAdded()
        {
            var provider = Init(services =>
            {
            });

            var manager = provider.GetRequiredService<ICloudEventDefinitionManager>();
            manager.TryAdd(new CloudEventDefinition("test", "another"));
            manager.TryAdd(new CloudEventDefinition("test", "another"));
            
            Assert.Single(manager.List());
        }
        
        [Fact]
        public void CanRemove()
        {
            var provider = Init(services =>
            {
            });

            var manager = provider.GetRequiredService<ICloudEventDefinitionManager>();
            manager.TryAdd(new CloudEventDefinition("test", "another"));
            Assert.Single(manager.List());

            manager.TryRemove(new CloudEventDefinition("test", "another"));
            Assert.Empty(manager.List());
        }
        
        [Fact]
        public void CanUpdate()
        {
            var provider = Init(services =>
            {
            });

            var manager = provider.GetRequiredService<ICloudEventDefinitionManager>();
            manager.TryAdd(new CloudEventDefinition("test", "another"));
            Assert.Single(manager.List());

            manager.AddOrUpdate(new CloudEventDefinition("test", "another", new Uri("https://localhost")));
            Assert.Single(manager.List());

            var def = manager.List().First();
            Assert.Equal(new Uri("https://localhost"), def.DataSchemaUri);
        }
    }
}
