using System.Collections.Generic;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventRouteCollection
    {
        IEnumerable<ICloudEventRoute> Routes { get; }
    }
}
