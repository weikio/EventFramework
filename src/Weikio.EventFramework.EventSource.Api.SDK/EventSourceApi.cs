using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Weikio.EventFramework.EventSource.Api.SDK
{
    public class EventSourceApi
    {
        private readonly IServiceProvider _serviceProvider;
        
        public EventSourceApiConfiguration Configuration { get; set; }

        public EventSourceApi(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Handle()
        {
            var handler = ActivatorUtilities.CreateInstance(_serviceProvider, Configuration.ApiType);
            
            
        }
    }
}
