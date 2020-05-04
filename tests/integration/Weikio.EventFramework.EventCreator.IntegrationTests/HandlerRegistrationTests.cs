using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventAggregator.Core;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.Extensions.EventAggregator;
using Xunit;

namespace Weikio.EventFramework.EventCreator.IntegrationTests
{
    public class HandlerRegistrationTests: EventAggregatorTestBase, IDisposable
    {
        public HandlerRegistrationTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }
        
        [Fact]
        public async Task CanRegisterAction()
        {
            var handledMessages = 0;
            var publisher = Init(services =>
            {
                services.AddHandler(ev =>
                {
                    handledMessages += 1;
                });
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent());
            
            // Assert
            Assert.Equal(1, handledMessages);
        }
        
        [Fact]
        public async Task CanRegisterAsyncAction()
        {
            var handledMessages = 0;
            var publisher = Init(services =>
            {
                services.AddHandler(async ev =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    handledMessages += 1;
                });
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent());
            
            // Assert
            Assert.Equal(1, handledMessages);
        }
        
        [Fact]
        public async Task CanRegisterAsyncActionWithTypeFilter()
        {
            var handledMessages = 0;
            var publisher = Init(services =>
            {
                services.AddHandler(async ev =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    handledMessages += 1;
                }, eventType: "CustomerCreatedEvent");
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent());
            await publisher.Publish(new CustomerDeletedEvent());
            
            // Assert
            Assert.Equal(1, handledMessages);
        }
        
        [Fact]
        public async Task CanRegisterClassWithHandle()
        {
            var publisher = Init(services =>
            {
                services.AddHandler<TestHandler>();
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent());
            
            // Assert
            Assert.Equal(1, TestHandler.HandleCount);
        }
        
        [Fact]
        public async Task CanRegisterClassWithTypeFilter()
        {
            var publisher = Init(services =>
            {
                services.AddHandler<TestHandler>(eventType: "CustomerCreatedEvent");
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent());
            await publisher.Publish(new CustomerDeletedEvent());

            // Assert
            Assert.Equal(1, TestHandler.HandleCount);
        }
        
        [Fact]
        public async Task CanRegisterClassWithTypeFilterInHandle()
        {
            var publisher = Init(services =>
            {
                services.AddHandler<TestHandlerWithFilterInHandle>();
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent());
            await publisher.Publish(new CustomerDeletedEvent());

            // Assert
            Assert.Equal(1, TestHandlerWithFilterInHandle.HandleCount);
        }
        
        [Fact]
        public async Task CanRegisterClassWithPredicate()
        {
            var publisher = Init(services =>
            {
                services.AddHandler<TestHandler>(canHandle: ev => string.Equals(ev.Type, "CustomerCreatedEvent"));
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent());
            await publisher.Publish(new CustomerDeletedEvent());

            // Assert
            Assert.Equal(1, TestHandler.HandleCount);
        }
        
        [Fact]
        public async Task CanRegisterClassWithCriteria()
        {
            var publisher = Init(services =>
            {
                services.AddHandler<TestHandler>(criteria: new CloudEventCriteria()
                {
                    Type =  "CustomerCreatedEvent"
                });
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent());
            await publisher.Publish(new CustomerDeletedEvent());

            // Assert
            Assert.Equal(1, TestHandler.HandleCount);
        }
        
        [Fact]
        public async Task CanRegisterType()
        {
            var publisher = Init(services =>
            {
                services.AddHandler(typeof(TestHandler));
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent());

            // Assert
            Assert.Equal(1, TestHandler.HandleCount);
        }
        
        [Fact]
        public async Task CanRegisterTypeWithPredicate()
        {
            var publisher = Init(services =>
            {
                services.AddHandler(typeof(TestHandler), canHandle: ev => Task.FromResult(string.Equals(ev.Type, "CustomerCreatedEvent")));
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent());
            await publisher.Publish(new CustomerDeletedEvent());

            // Assert
            Assert.Equal(1, TestHandler.HandleCount);
        }
        
        [Fact]
        public async Task CanRegisterClassWithCloudEventArgument()
        {
            var publisher = Init(services =>
            {
                services.AddHandler<TestHandlerWithCloudEventArgument>();
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent());

            // Assert
            Assert.Equal("CustomerCreatedEvent", TestHandlerWithCloudEventArgument.HandledEventType);
        }
        
        [Fact]
        public async Task CanRegisterClassWithGenericCloudEventArgument()
        {
            var publisher = Init(services =>
            {
                services.AddHandler<TestHandlerWithGenericCloudEventArgument>();
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent(){Name = "Test Customer"});

            // Assert
            Assert.Equal("Test Customer", TestHandlerWithGenericCloudEventArgument.CreatedCustomer);
        }
        
        [Fact]
        public async Task CanRegisterClassWithTypeArgument()
        {
            var publisher = Init(services =>
            {
                services.AddHandler<TestHandlerWithTypeArgument>();
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent(){Name = "Test Customer"});

            // Assert
            Assert.Equal("Test Customer", TestHandlerWithTypeArgument.CreatedCustomer);
        }
        
        [Fact]
        public async Task CanRegisterClassWithGuard()
        {
            var publisher = Init(services =>
            {
                services.AddHandler<TestHandlerWithGuard>();
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent());

            // Assert
            Assert.Equal(0, TestHandlerWithGuard.HandleCount);
        }
        
        [Fact]
        public async Task CanRegisterClassWithGenericGuard()
        {
            var publisher = Init(services =>
            {
                services.AddHandler<TestHandlerWithGenericGuard>();
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent() {Name = "Test Customer"});
            await publisher.Publish(new CustomerCreatedEvent() {Name = "Another Test"});
            await publisher.Publish(new CustomerCreatedEvent() {Name = "Test Customer"});

            // Assert
            Assert.Equal(2, TestHandlerWithGenericGuard.HandleCount);
        }
        
        [Fact]
        public async Task CanRegisterClassWithTypedGuard()
        {
            var publisher = Init(services =>
            {
                services.AddHandler<TestHandlerWithTypedGuard>();
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent() {Name = "Test Customer"});
            await publisher.Publish(new CustomerCreatedEvent() {Name = "Another Test"});
            await publisher.Publish(new CustomerCreatedEvent() {Name = "Test Customer"});

            // Assert
            Assert.Equal(2, TestHandlerWithTypedGuard.HandleCount);
        }
        
        [Fact]
        public async Task CanRegisterClassWithTypedGuardAndWithCanHandle()
        {
            var counter = 0;
            var publisher = Init(services =>
            {
                services.AddHandler<TestHandlerWithTypedGuard>(canHandle: ev =>
                {
                    var result = (decimal)counter % 2 == 0;
                    counter += 1;

                    return result;
                } );
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent() {Name = "Test Customer"});
            await publisher.Publish(new CustomerCreatedEvent() {Name = "Test Customer"});
            await publisher.Publish(new CustomerCreatedEvent() {Name = "Test Customer"});
            await publisher.Publish(new CustomerCreatedEvent() {Name = "Test Customer"});
            await publisher.Publish(new CustomerCreatedEvent() {Name = "Another Test"});
            await publisher.Publish(new CustomerCreatedEvent() {Name = "Test Customer"});

            // Assert
            Assert.Equal(2, TestHandlerWithTypedGuard.HandleCount);
        }
        
        [Fact]
        public async Task CanRegisterClassWithMultipleHandlers()
        {
            var publisher = Init(services =>
            {
                services.AddHandler<TestHandlerWithMultipleHandlers>();
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent());
            await publisher.Publish(new CustomerCreatedEvent());
            await publisher.Publish(new CustomerDeletedEvent());

            // Assert
            Assert.Equal(2, TestHandlerWithMultipleHandlers.HandleCreatedCount);
            Assert.Equal(1, TestHandlerWithMultipleHandlers.HandleDeletedCount);
        }
        
        [Fact]
        public async Task CanRegisterClassWithMultipleHandlersAndGuardMethods()
        {
            var publisher = Init(services =>
            {
                services.AddHandler<TestHandlerWithMultipleHandlersAndGuardMethods>();
            });
            
            // Act
            await publisher.Publish(new CustomerCreatedEvent() { Name = "Test Customer" });
            await publisher.Publish(new CustomerCreatedEvent() { Name = "Test Customer" });
            await publisher.Publish(new CustomerCreatedEvent() { Name = "Another Test" });

            await publisher.Publish(new CustomerDeletedEvent() { Name = "Test Customer" });
            await publisher.Publish(new CustomerDeletedEvent() { Name = "Another Test" });
            
            // Assert
            Assert.Equal(2, TestHandlerWithMultipleHandlersAndGuardMethods.HandleCreatedCount);
            Assert.Equal(1, TestHandlerWithMultipleHandlersAndGuardMethods.HandleDeletedCount);
        }

        public void Dispose()
        {
            TestHandler.HandleCount = 0;
            TestHandlerWithTypedGuard.HandleCount = 0;
        }
    }

    public class TestHandler
    {
        public static int HandleCount { get; set; }
        
        public Task Handle()
        {
            HandleCount += 1;
            return Task.CompletedTask;
        }
    }
    
    public class TestHandlerWithCloudEventArgument
    {
        public static string HandledEventType { get; set; }
        
        public Task Handle(CloudEvent cloudEvent)
        {
            HandledEventType = cloudEvent.Type;
            return Task.CompletedTask;
        }
    }
    
    public class TestHandlerWithFilterInHandle
    {
        public static int HandleCount { get; set; }
        
        public Task Handle(CloudEvent cloudEvent, string eventType = "CustomerCreatedEvent")
        {
            HandleCount += 1;
            return Task.CompletedTask;
        }
    }
    
    public class TestHandlerWithGenericCloudEventArgument
    {
        public static string CreatedCustomer { get; set; }
        
        public Task Handle(CloudEvent<CustomerCreatedEvent> customerCreated)
        {
            CreatedCustomer = customerCreated.Object.Name;
            return Task.CompletedTask;
        }
    }
    
    public class TestHandlerWithTypeArgument
    {
        public static string CreatedCustomer { get; set; }
        
        public Task Handle(CustomerCreatedEvent customerCreated)
        {
            CreatedCustomer = customerCreated.Name;
            return Task.CompletedTask;
        }
    }
    
    public class TestHandlerWithGuard
    {
        public static int HandleCount { get; set; }

        public Task<bool> CanHandle(CloudEvent ev)
        {
            return Task.FromResult(false);
        }
        
        public Task Handle()
        {
            HandleCount += 1;
            return Task.CompletedTask;
        }
    }
    
    public class TestHandlerWithGenericGuard
    {
        public static int HandleCount { get; set; }

        public Task<bool> CanHandle(CloudEvent<CustomerCreatedEvent> ev)
        {
            return Task.FromResult(ev.Object.Name == "Test Customer");
        }
        
        public Task Handle(CloudEvent<CustomerCreatedEvent> ev)
        {
            HandleCount += 1;
            return Task.CompletedTask;
        }
    }
    
    public class TestHandlerWithTypedGuard
    {
        public static int HandleCount { get; set; }

        public Task<bool> CanHandle(CustomerCreatedEvent ev)
        {
            return Task.FromResult(ev.Name == "Test Customer");
        }
        
        public Task Handle(CustomerCreatedEvent ev)
        {
            HandleCount += 1;
            return Task.CompletedTask;
        }
    }
    
    public class TestHandlerWithMultipleHandlers
    {
        public static int HandleCreatedCount { get; set; }
        public static int HandleDeletedCount { get; set; }
        
        public Task Handle(CustomerCreatedEvent ev, string eventType = "CustomerCreatedEvent")
        {
            HandleCreatedCount += 1;
            return Task.CompletedTask;
        }
        
        public Task Handle(CustomerDeletedEvent ev, string eventType = "CustomerDeletedEvent")
        {
            HandleDeletedCount += 1;
            return Task.CompletedTask;
        }
    }
    
    public class TestHandlerWithMultipleHandlersAndGuardMethods
    {
        public static int HandleCreatedCount { get; set; }
        public static int HandleDeletedCount { get; set; }
        
        public Task<bool> CanHandleCustomerCreated(CustomerCreatedEvent ev)
        {
            return Task.FromResult(ev.Name == "Test Customer");
        }
        
        public Task<bool> CanHandleCustomerDeleted(CustomerDeletedEvent ev)
        {
            return Task.FromResult(ev.Name == "Test Customer");
        }
        
        public Task HandleCustomerCreated(CustomerCreatedEvent ev, string eventType = "CustomerCreatedEvent")
        {
            HandleCreatedCount += 1;
            return Task.CompletedTask;
        }
        
        public Task HandleCustomerDeleted(CustomerDeletedEvent ev, string eventType = "CustomerDeletedEvent")
        {
            HandleDeletedCount += 1;
            return Task.CompletedTask;
        }
    }
}
