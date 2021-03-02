using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventGateway
{
    public class CloudEventGatewayManager : List<ICloudEventGateway>, ICloudEventGatewayManager
    {
        private readonly ICloudEventGatewayInitializer _initializer;

        public CloudEventGatewayManager(ICloudEventGatewayInitializer initializer)
        {
            _initializer = initializer;
        }

        public IEnumerable<ICloudEventGateway> Gateways => this;

        public ICloudEventGateway Get(string gatewayName)
        {
            var result = this.FirstOrDefault(x => string.Equals(gatewayName, x.Name, StringComparison.InvariantCultureIgnoreCase));

            if (result == null)
            {
                if (Count == 1)
                {
                    return this.Single();
                }

                if (Count == 0)
                {
                    throw new NoGatewaysConfiguredException();
                }

                throw new UnknownGatewayException(gatewayName);
            }

            return result;
        }

        public void Add(string gatewayName, ICloudEventGateway gateway)
        {
            var existing = this.FirstOrDefault(x => string.Equals(gatewayName, x.Name, StringComparison.InvariantCultureIgnoreCase));

            if (existing != null)
            {
                throw new DuplicateGatewayException(gatewayName);
            }

            Add(gateway);
        }

        public async Task Update()
        {
            var removed = new List<ICloudEventGateway>();

            foreach (var cloudEventGateway in Gateways)
            {
                if (cloudEventGateway.Status == CloudEventGatewayStatus.Changed)
                {
                }
                else if (cloudEventGateway.Status == CloudEventGatewayStatus.New)
                {
                    await _initializer.Initialize(cloudEventGateway);
                }
                else if (cloudEventGateway.Status == CloudEventGatewayStatus.Removed)
                {
                    //cloudEventGateway.Dispose();
                    removed.Add(cloudEventGateway);
                }
            }

            foreach (var cloudEventGateway in removed)
            {
                Remove(cloudEventGateway);
            }
        }
    }
}
