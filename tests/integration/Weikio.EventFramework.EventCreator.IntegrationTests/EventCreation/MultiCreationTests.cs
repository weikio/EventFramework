using System.Collections.Generic;
using System.Linq;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;
using Xunit;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.EventCreation
{
    public class MultiCreationTests : EventCreationTestBase
    {
        private List<object> _objects;

        public MultiCreationTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
            _objects = new List<object>
            {
                new CustomerCreatedEvent() { Name = "first" },
                new CustomerCreatedEvent() { Name = "second" },
                new CustomerCreatedEvent() { Name = "third" }
            };
        }

        [Fact]
        public void CanCreateMultipleEvents()
        {
            var server = Init();
        
            // Act 
            var result = server.CreateCloudEvents(_objects);
        
            // Assert
            Assert.Equal(3, result.Count());
        }
        
        [Fact]
        public void CorrectlyContainsSequence()
        {
            var server = Init();
        
            // Act 
            var result = (server.CreateCloudEvents(_objects)).ToList();
        
            // Assert
            for (var index = 0; index < result.Count; index++)
            {
                dynamic cloudEvent = result[index];

                var attributes = cloudEvent.GetAttributes();
                var sequence = int.Parse(attributes["sequence"]);
                
                Assert.Equal(sequence, index);
            }
        }
    }
}
