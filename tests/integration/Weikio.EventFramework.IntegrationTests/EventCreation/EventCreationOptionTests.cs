using System;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.IntegrationTests.EventSource;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;

namespace Weikio.EventFramework.IntegrationTests.EventCreation
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
        
        [Fact]
        public void CanConfigureDefaultOptions()
        {
            throw new NotImplementedException();
            // Not yet working. DefaultCloudEventCreatorOptionsProvider returns "new" instead of the configured default options.
            // Should use something like "IsConfigured" in CloudEventCreationOptions
            var server = Init(services =>
            {
                services.Configure<CloudEventCreationOptions>(options =>
                {
                    options.GetEventTypeName = (creationOptions, provider, eventObject) =>
                    {
                        if (eventObject.GetType() == typeof(CustomerCreatedEvent))
                        {
                            return "custom";
                        }

                        return "default";
                    };
                });
            });
            
            // Act 
            var result = server.CreateCloudEvent(new CustomerCreatedEvent() { Name = "John Smith" });
            var result2 = server.CreateCloudEvent(new CustomerDeletedEvent() { Name = "John Smith" });

            // Assert
            Assert.Equal("custom", result.Type);
            Assert.Equal("default", result2.Type);
        }
    }
}
