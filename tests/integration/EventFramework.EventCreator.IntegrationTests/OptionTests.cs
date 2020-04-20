using System.Threading.Tasks;
using ApiFramework.IntegrationTests;
using EventCreation;
using EventFramework.EventCreator.IntegrationTests.Events;
using EventFramework.EventCreator.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.EventFramework.EventCreator;
using Xunit;

namespace EventFramework.EventCreator.IntegrationTests
{
    public class OptionTests : EventCreationTestBase
    {
        public OptionTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
            ObjectFactory = () => new CustomerCreatedEvent() { Name = "John Smith" };
        }

        [Fact]
        public async Task CanUseDefaultOptions()
        {
            var server = Init();

            // Act 
            var result = await server.GetSingle();

            // Assert
            Assert.Equal(nameof(CustomerCreatedEvent), result.Type);
        }

        [Fact]
        public async Task CanConfigureEventType()
        {
            var server = Init(services =>
            {
                services.ConfigureCloudEvent<CustomerCreatedEvent>(options =>
                {
                    options.EventTypeName = "Hello";
                });
            });

            // Act 
            var result = await server.GetSingle();

            // Assert
            Assert.Equal("Hello", result.Type);
        }

        [Fact]
        public async Task CanConfigureSubject()
        {
            var server = Init(services =>
            {
                services.ConfigureCloudEvent<CustomerCreatedEvent>(options =>
                {
                    options.GetSubject = (creationOptions, provider, arg3) =>
                    {
                        var ev = (CustomerCreatedEvent) arg3;

                        return ev.Name;
                    };
                });
            });

            // Act 
            var result = await server.GetSingle();

            // Assert
            Assert.Equal("John Smith", result.Subject);
        }
    }
}
