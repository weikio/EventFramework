using System.Collections.Generic;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Extensions
{
    public class CloudEventRouteCollection : List<ICloudEventRoute>, ICloudEventRouteCollection
    {
        public IEnumerable<ICloudEventRoute> Routes => this;
    }
}
