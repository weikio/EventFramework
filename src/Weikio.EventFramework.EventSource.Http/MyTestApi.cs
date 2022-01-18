using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Api;
using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventSource.Http
{
    public class MyTestApi : IApiEventSource<MyTestApiConfiguration>
    {
        public MyTestApiConfiguration Configuration { get; set; }

        public async Task<IActionResult> Handle(ICloudEventPublisher cloudEventPublisher)
        {
            var ev = new MyTestEvent();
            
            await cloudEventPublisher.Publish(ev);
            
            return new OkResult();
        }
    }
}
