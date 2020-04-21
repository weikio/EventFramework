using Microsoft.AspNetCore.Mvc;

namespace EventFrameworkTestBed.Router
{
    [Route("router/endpoint")]
    public class SingleEventController : ControllerBase
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
