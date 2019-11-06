using System;
using System.Collections.Generic;
using System.Linq;

namespace Weikio.EventFramework.Abstractions
{
    public class CloudEventGatewayCollection : List<ICloudEventGateway>, ICloudEventGatewayCollection
    {
        public CloudEventGatewayCollection(IEnumerable<ICloudEventGateway> gateways)
        {
            AddRange(gateways);
        }

        public IEnumerable<ICloudEventGateway> Gateways => this;
        
        public ICloudEventGateway Get(string gatewayName)
        {
            var result = this.FirstOrDefault(x => string.Equals(gatewayName, x.Name, StringComparison.InvariantCultureIgnoreCase));

            if (result == null)
            {
                throw new UnknownGatewayException(gatewayName);
            }

            return result;
        }

        public void Add(string gatewayName, ICloudEventGateway gateway)
        {
            var existing = this.FirstOrDefault(x => string.Equals(gatewayName, x.Name, StringComparison.InvariantCultureIgnoreCase));

            if (existing == null)
            {
                throw new DuplicateGatewayException(gatewayName);
            }
            
            Add(gateway);
        }
    }
}
