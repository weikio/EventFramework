using System.Collections.Generic;

namespace Weikio.EventFramework.Abstractions
{
    public class CloudEventHandlerCollection : List<ICloudEventHandler>, ICloudEventHandlerCollection
    {
        public IEnumerable<ICloudEventHandler> Handlers => this;
    }
}
