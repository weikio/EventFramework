using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;
using Xunit;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.EventCreation
{
    public class EventCreationOptionTests : EventCreationTestBase
    {
        public EventCreationOptionTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public void CanUseDefaultOptions()
        {
            var server = Init();

            // Act 
            var result = server.CreateCloudEvent(new CustomerCreatedEvent() { Name = "John Smith" });

            // Assert
            Assert.Equal(nameof(CustomerCreatedEvent), result.Type);
        }

        [Fact]
        public void CanConfigureEventType()
        {
            var server = Init(services =>
            {
                services.ConfigureCloudEvent<CustomerCreatedEvent>(options =>
                {
                    options.EventTypeName = "Hello";
                });
            });
        
            // Act 
            var result = server.CreateCloudEvent(new CustomerCreatedEvent() { Name = "John Smith" });
        
            // Assert
            Assert.Equal("Hello", result.Type);
        }
        
        [Fact]
        public void CanConfigureSubject()
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
            var result = server.CreateCloudEvent(new CustomerCreatedEvent() { Name = "John Smith" });
        
            // Assert
            Assert.Equal("John Smith", result.Subject);
        }
    }
}
