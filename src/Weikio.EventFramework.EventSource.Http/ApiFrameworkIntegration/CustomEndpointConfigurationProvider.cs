using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.SDK;

namespace Weikio.EventFramework.EventGateway.Http.ApiFrameworkIntegration
{
    public class CustomEndpointConfigurationProvider : IEndpointConfigurationProvider
    {
        private readonly List<EndpointDefinition> _endpointDefinitions = new List<EndpointDefinition>();

        public Task<List<EndpointDefinition>> GetEndpointConfiguration()
        {
            return Task.FromResult(_endpointDefinitions);
        }

        public void Add(EndpointDefinition definition)
        {
            _endpointDefinitions.Add(definition);
        }
    }
}
