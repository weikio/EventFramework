using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Gateways;

namespace Weikio.EventFramework.AspNetCore.Infrastructure
{
    public class HttpGatewayChangeToken
    {
        public void Initialize()
        {
            TokenSource = new CancellationTokenSource();
        }

        public CancellationTokenSource TokenSource { get; private set; } = new CancellationTokenSource();
    }
    
    public class HttpGatewayActionDescriptorChangeProvider : IActionDescriptorChangeProvider
    {
        public HttpGatewayActionDescriptorChangeProvider(HttpGatewayChangeToken changeToken)
        {
            ChangeToken = changeToken;
        }

        public HttpGatewayChangeToken ChangeToken { get; }

        public IChangeToken GetChangeToken()
        {
            if (ChangeToken.TokenSource.IsCancellationRequested)
            {
                ChangeToken.Initialize();

                return new CancellationChangeToken(ChangeToken.TokenSource.Token);
            }

            return new CancellationChangeToken(ChangeToken.TokenSource.Token);
        }
    }
    
    public class HttpGatewayChangeNotifier
    {
        private readonly HttpGatewayChangeToken _changeToken;

        public HttpGatewayChangeNotifier(HttpGatewayChangeToken changeToken)
        {
            _changeToken = changeToken;
        }

        public void Notify()
        {
            _changeToken.TokenSource.Cancel();
        }
    }
    
    public class ApiFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly ICloudEventGatewayCollection _cloudEventGatewayCollection;

        public ApiFeatureProvider(ICloudEventGatewayCollection cloudEventGatewayCollection)
        {
            _cloudEventGatewayCollection = cloudEventGatewayCollection;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var gatewaysWithIncomingHttpChannel = _cloudEventGatewayCollection.Gateways?.Where(x => x.SupportsIncoming)
                .Where(x => x.IncomingChannel is IncomingHttpChannel).Select(x => x.IncomingChannel).Cast<IncomingHttpChannel>();

            if (gatewaysWithIncomingHttpChannel?.Any() != true)
            {
                return;
            }
            
            var controllerTypes = _cloudEventGatewayCollection.Endpoints
                .SelectMany(p => p.ApiTypes)
                .ToArray();

            foreach (var controllerType in controllerTypes)
            {
                var existing = feature.Controllers.FirstOrDefault(x => x.AsType() == controllerType);

                if (existing != null)
                {
                    continue;
                }

                feature.Controllers.Add(controllerType.GetTypeInfo());
            }
        }
    }
}
