namespace Weikio.EventFramework.EventSource.DataSources.CosmosDB
{
    public class CosmosDBEventSourceInstanceDataSourceOptions
    {
        public string DocumentDbUri { get; set; }
        public string DocumentDbKey { get; set; }
        public string DatabaseId { get; set; }
        public string CollectionId { get; set; }
        public bool IsDefaultStateStore { get; set; } = true;
    }
}