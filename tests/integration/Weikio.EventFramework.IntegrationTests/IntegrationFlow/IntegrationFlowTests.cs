using System.Threading.Tasks;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.EventFramework.IntegrationFlow;
using Weikio.EventFramework.IntegrationTests.EventSource.Sources;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.IntegrationFlow
{
    public class IntegrationFlowBuilderTests : IntegrationFlowTestBase
    {
        public IntegrationFlowBuilderTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public void CanCreateFlowBuilderUsingResourceName()
        {
            var server = Init();

            var flowBuilder = IntegrationFlowBuilder.From("hello");
        }
        
        [Fact]
        public void CanCreateFlowBuilderUsingNewEventSource()
        {
            var server = Init();

            var flowBuilder = IntegrationFlowBuilder.From<TestEventSource>();
        }
    }
}
