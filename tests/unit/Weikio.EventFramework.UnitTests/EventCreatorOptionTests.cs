using System;
using System.Collections.Generic;
using Weikio.EventFramework.EventCreator;
using Xunit;

namespace Weikio.EventFramework.UnitTests
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
        public void CanCreateMultipleEventsWithSequence()
        {
            var objs = new List<CustomerCreated>();

            for (var i = 0; i < 10; i++)
            {
                var obj = new CustomerCreated(Guid.NewGuid(), "Test", "User " + i);
                objs.Add(obj);
            }

            var cloudEvents = CloudEventCreator.Create(objs);

            foreach (var cloudEvent in cloudEvents)
            {
                var attributes = cloudEvent.GetAttributes();
                Assert.Contains("sequence", attributes);
            }
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

            var result = CloudEventCreator.Create(obj, new CloudEventCreationOptions
            {
                GetSubject = (options, provider, ob) =>
                {
                    var ev = (CustomerCreated) ob;

                    return $"{ev.FirstName} {ev.LastName}";
                }
            });

            Assert.Equal("Test User", result.Subject);
        }
    }
}