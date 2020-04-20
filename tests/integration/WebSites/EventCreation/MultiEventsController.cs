using System.Collections.Generic;
using System.Net.Mime;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventCreator;

namespace EventCreation
{
    [Route("multi")]
    public class MultiEventsController : ControllerBase
    {
        private readonly ObjectFactoryOptions _factory;
        private readonly ICloudEventCreator _cloudEventCreator;

        public MultiEventsController(ICloudEventCreator cloudEventCreator, IOptions<ObjectFactoryOptions> options)
        {
            _cloudEventCreator = cloudEventCreator;
            _factory = options.Value;
        }

        [HttpGet]
        public ActionResult<IEnumerable<byte[]>> Test()
        {
            var objs = _factory.CreateMulti();
            var result = _cloudEventCreator.CreateCloudEvents(objs);
            var f = new JsonEventFormatter();
            var resultBytes = new List<byte[]>();

            foreach (var cloudEvent in result)
            {
                ContentType contentType;
                var bytes = f.EncodeStructuredEvent(cloudEvent, out contentType);
                
                resultBytes.Add(bytes);
            }
                
            return Ok(resultBytes);
        }
    }
}
