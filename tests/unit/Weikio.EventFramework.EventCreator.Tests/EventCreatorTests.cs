using System;
using Xunit;

namespace Weikio.EventFramework.EventCreator.Tests
{
    public class EventCreatorOptionTests
    {
        [Fact]
        public void CanConfigureType()
        {
            var obj = new CustomerCreated(Guid.NewGuid(), "Test", "User");

            var result = CloudEventCreator.Create(obj, new CloudEventCreationOptions { EventTypeName = "hello-world" });

            Assert.Equal("hello-world", result.Type);
        }

        [Fact]
        public void CanConfigureSubject()
        {
            var obj = new CustomerCreated(Guid.NewGuid(), "Test", "User");

            var result = CloudEventCreator.Create(obj, new CloudEventCreationOptions { Subject = "Test User" });

            Assert.Equal("Test User", result.Subject);
        }

        [Fact]
        public void CanConfigureId()
        {
            var id = Guid.NewGuid();
            var obj = new CustomerCreated(id, "Test", "User");

            var result = CloudEventCreator.Create(obj, new CloudEventCreationOptions() { GetId = (options, provider, o) => id.ToString() });

            Assert.Equal(id.ToString(), result.Id);
        }
        
        [Fact]
        public void CanConfigureSubjectUsingObject()
        {
            var obj = new CustomerCreated(Guid.NewGuid(), "Test", "User");

            var result = CloudEventCreator.Create(obj, new CloudEventCreationOptions { GetSubject = (options, provider, ob) =>
            {
                var ev = (CustomerCreated) ob;

                return $"{ev.FirstName} {ev.LastName}";
            } });

            Assert.Equal("Test User", result.Subject);
        }
    }

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
    }
}
