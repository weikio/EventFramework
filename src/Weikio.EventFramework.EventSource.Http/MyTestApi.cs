using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventSource.Http
{
    public class MyTestApi : IApiEventSource<MyTestApiConfiguration>
    {
        public MyTestApiConfiguration Configuration { get; set; }
        
        public async Task<IActionResult> Handle()
        {
            var ev = new MyTestEvent();
            
            await Configuration.CloudEventPublisher.Publish(ev);
            
            return new OkResult();
        }
    }
}
