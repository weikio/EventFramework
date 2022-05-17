using Adafy.Candy.Entity;

namespace Weikio.EventFramework.EventSource.DataSources.CosmosDB
{
    public class StateEntity : Entity
    {
        public string EventSourceInstanceId { get; set; }
        public string State { get; set; }
    }
}