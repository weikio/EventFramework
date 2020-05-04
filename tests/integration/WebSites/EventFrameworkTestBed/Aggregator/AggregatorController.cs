using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc;
using Weikio.EventFramework.EventPublisher;

namespace EventFrameworkTestBed.Aggregator
{
    [Route("aggregator")]
    public class AggregatorController : ControllerBase
    {
        private readonly ICloudEventPublisher _cloudEventPublisher;

        public AggregatorController(ICloudEventPublisher cloudEventPublisher)
        {
            _cloudEventPublisher = cloudEventPublisher;
        }

        [HttpGet]
        public ActionResult Publish()
        {
            _cloudEventPublisher.Publish(new CustomerCreatedEvent() { Name = "Test user" });
            return Ok();
        }
    }
}
