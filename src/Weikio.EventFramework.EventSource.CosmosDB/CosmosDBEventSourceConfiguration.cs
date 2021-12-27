using System;

namespace Weikio.EventFramework.EventSource.CosmosDB
{
    public class CosmosDBEventSourceConfiguration
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string Container { get; set; }
        public int PollingDelayInSeconds { get; set; } = 5;
    }
}
