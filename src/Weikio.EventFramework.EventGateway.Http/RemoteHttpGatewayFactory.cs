using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class RemoteHttpGatewayFactory
    {
        private readonly IOptionsMonitor<RemoteHttpGatewayOptions> _optionsMonitor;
        private readonly IServiceProvider _serviceProvider;

        public RemoteHttpGatewayFactory(IOptionsMonitor<RemoteHttpGatewayOptions> optionsMonitor, IServiceProvider serviceProvider)
        {
            _optionsMonitor = optionsMonitor;
            _serviceProvider = serviceProvider;
        }

        public ICloudEventGateway Create(string name)
        {
            var options = _optionsMonitor.Get(name);

            if (options.ClientFactory == null)
            {
                options.ClientFactory = () => 
                {
                    var factory = _serviceProvider.GetRequiredService<IHttpClientFactory>();

                    return factory.CreateClient(options.Name);
                };
            }
            
            return new RemoteHttpGateway(options);
        }
    }
}
