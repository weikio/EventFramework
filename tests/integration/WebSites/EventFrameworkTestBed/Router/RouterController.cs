using Microsoft.AspNetCore.Mvc;
using Weikio.EventFramework.EventPublisher;

namespace EventFrameworkTestBed.Router
{
    [Route("router/endpoint")]
    public class RouterController : ControllerBase
    {
        private static int _callCount = 0;

        [HttpPost]
        public ActionResult Endpoint()
        {
            _callCount += 1;

            return Ok();
        }

        [HttpGet]
        public ActionResult<int> GetCallCount()
        {
            return Ok(_callCount);
        }
    }
}
