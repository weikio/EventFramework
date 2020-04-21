using System.Collections.Generic;
using System.Threading.Tasks;
using ApiFramework.IntegrationTests;
using CloudNative.CloudEvents.Extensions;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventCreator.IntegrationTests.Events;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;
using Xunit;

namespace Weikio.EventFramework.EventCreator.IntegrationTests
{
    public class MultiCreationTests : EventCreationTestBase
    {
        public MultiCreationTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
            MultiObjectFactory = () =>
            {
                var result = new List<object>
                {
                    new CustomerCreatedEvent() { Name = "first" },
                    new CustomerCreatedEvent() { Name = "second" },
                    new CustomerCreatedEvent() { Name = "third" }
                };

                return result;
            };
        }

        [Fact]
        public async Task CanCreateMultipleEvents()
        {
            var server = Init();

            // Act 
            var result = await server.GetMulti();

            // Assert
            Assert.Equal(3, result.Count);
        }
        
        [Fact]
        public async Task CorrectlyContainsSequence()
        {
            var server = Init();

            // Act 
            var result = await server.GetMulti();

            // Assert
            for (var index = 0; index < result.Count; index++)
            {
                var cloudEvent = result[index];

                var seq = cloudEvent.sequence;
                var sequence = int.Parse(seq);
                Assert.Equal(sequence, index);
            }
        }
    }
}
