using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventCreator;

namespace EventCreation
{
    [Route("single")]
    public class SingleEventController : ControllerBase
    {
        private readonly ObjectFactoryOptions _factory;
        private readonly ICloudEventCreator _cloudEventCreator;

        public SingleEventController( ICloudEventCreator cloudEventCreator, IOptions<ObjectFactoryOptions> options)
        {
            _cloudEventCreator = cloudEventCreator;
            _factory = options.Value;
        }

        [HttpGet]
        public ActionResult<CloudEvent> Test()
        {
            var obj = _factory.Create();
            var result = _cloudEventCreator.CreateCloudEvent(obj);

            return Ok(result);
        }
    }
}
