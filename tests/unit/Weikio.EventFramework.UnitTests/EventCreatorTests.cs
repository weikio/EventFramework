﻿using System;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.Tests.Shared;
using Xunit;

namespace Weikio.EventFramework.UnitTests
{
    public class EventCreatorTests
    {
        [Fact]
        public void CanConvertObjectToCloudEvent()
        {
            var obj = new CustomerCreated(Guid.NewGuid(), "Test", "User");

            var result = CloudEventCreator.Create(obj);

            Assert.Equal(typeof(CustomerCreated).Name, result.Type);
        }

        [Fact]
        public void CanCustomizeEventType()
        {
            var obj = new CustomerCreated(Guid.NewGuid(), "Test", "User");

            var result = CloudEventCreator.Create(obj, eventTypeName: "hello-world");

            Assert.Equal("hello-world", result.Type);
        }

        [Fact]
        public void CanCustomizeSubject()
        {
            var obj = new CustomerCreated(Guid.NewGuid(), "Test", "User");

            var result = CloudEventCreator.Create(obj, subject: "Test User");

            Assert.Equal("Test User", result.Subject);
        }

        [Fact]
        public void CanCustomizeId()
        {
            var id = Guid.NewGuid();
            var obj = new CustomerCreated(id, "Test", "User");

            var result = CloudEventCreator.Create(obj, id: id.ToString());

            Assert.Equal(id.ToString(), result.Id);
        }
        
        [Fact]
        public void CanDefineEventTypeUsingAttribute()
        {
            var obj = new OrderCreated();

            var result = CloudEventCreator.Create(obj);

            Assert.Equal("CustomerOrderCreated", result.Type);
        }
        
        [Fact]
        public void CanDefineEventSourceUsingAttribute()
        {
            var obj = new OrderCreated();

            var result = CloudEventCreator.Create(obj);

            Assert.Equal(new Uri("https://test.event"), result.Source);
        }

        [Fact]
        public void CanDefineEventSourceUsingAssemblyLevelAttribute()
        {
            var obj = new CustomerDeleted();

            var result = CloudEventCreator.Create(obj);

            Assert.Equal(new Uri("https://assembly.source"), result.Source);
        }

        [Fact]
        public void TypeLevelEventSourceOverridesAssemblyLevelSource()
        {
            var obj = new OrderCreated();

            var result = CloudEventCreator.Create(obj);

            Assert.Equal(new Uri("https://test.event"), result.Source);
        }
    }
}
