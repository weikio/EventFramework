using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Weikio.ApiFramework;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Apis;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.ApiFramework.SDK;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.EventGateway.Http.ApiFrameworkIntegration;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class HttpCloudEventReceiverApi
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _contextAccessor;

        public HttpCloudEventReceiverApi(IAuthorizationService authorizationService, 
            IHttpContextAccessor contextAccessor)
        {
            _authorizationService = authorizationService;
            _contextAccessor = contextAccessor;
        }

        public HttpCloudEventReceiverApiConfiguration Configuration { get; set; }

        public async Task<IActionResult> ReceiveEvent()
        {
            var httpContext = _contextAccessor.HttpContext;
            
            var jsonReader = new JsonTextReader(new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 8192, true));
            var jToken = await JToken.LoadAsync(jsonReader);

            var cloudEventFormatter = new JsonEventFormatter();

            CloudEvent[] receivedEvents;
            if (jToken is JArray jArray)
            {
                var events = new List<CloudEvent>();
                foreach (var token in jArray)
                {
                    var jObject = (JObject) token;
                    var cloudEvent = cloudEventFormatter.DecodeJObject(jObject);
                    
                    events.Add(cloudEvent);
                }

                receivedEvents = events.ToArray();
            }
            else if (jToken is JObject jObject)
            {
                var cloudEvent = cloudEventFormatter.DecodeJObject(jObject);
                receivedEvents = new[] { cloudEvent };
            }
            else
            {
                throw new Exception("Unknown content type");
            }
            
            if (Configuration == null)
            {
                return new StatusCodeResult(500);
            }
            
            // Assert policy
            if (!string.IsNullOrWhiteSpace(Configuration?.PolicyName))
            {
                var user = httpContext.User;

                var authResult = await _authorizationService.AuthorizeAsync(user, Configuration.PolicyName);

                if (!authResult.Succeeded)
                {
                    return new UnauthorizedResult();
                }
            }

            await Configuration.CloudEventPublisher.Publish(receivedEvents);
            
            return new OkResult();
        }
    }

    public static class EventFrameworkHttpGatewayExtensions
    {
        public static IEventFrameworkBuilder AddHttpGateways(this IEventFrameworkBuilder builder)
        {
            AddHttpGateways(builder.Services);

            return builder;
        }

        public static IServiceCollection AddHttpGateways(this IServiceCollection services)
        {
            services.AddCloudEventGateway();
            services.AddHealthChecks();
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            var customerEndpointConfigurationProvider = new CustomEndpointConfigurationProvider();
            services.AddSingleton(customerEndpointConfigurationProvider);

            if (services.All(x => x.ServiceType != typeof(IEndpointInitializer)))
            {
                // TODO: Collection concurrent problem in Api Framework
                services.TryAddSingleton<IEndpointInitializer, SyncEndpointInitializer>();

                services.AddApiFrameworkCore(options =>
                {
                    options.AutoResolveEndpoints = false;
                    options.EndpointHttpVerbResolver = new CustomHttpVerbResolver();
                    options.ApiCatalogs.Add(new TypeApiCatalog(typeof(HttpCloudEventReceiverApi)));
                });

                // services.AddSingleton<IEndpointConfigurationProvider>(provider =>
                // {
                //     var gatewayCollection = provider.GetRequiredService<ICloudEventGatewayManager>();
                //     var httpGateways = gatewayCollection.Gateways.OfType<HttpGateway>().ToList();
                //     var customConfigProvider = provider.GetRequiredService<CustomEndpointConfigurationProvider>();
                //
                //     foreach (var httpGateway in httpGateways)
                //     {
                //         var endpointDefinition = new EndpointDefinition(httpGateway.Endpoint, typeof(HttpCloudEventReceiverApi).FullName,
                //             new HttpCloudEventReceiverApiConfiguration() { GatewayName = httpGateway.Name }, new EmptyHealthCheck(), string.Empty);
                //
                //         customConfigProvider.Add(endpointDefinition);
                //     }
                //
                //     return customConfigProvider;
                // });
            }
            else
            {
                services.AddTransient<IPluginCatalog>(sp =>
                {
                    var typeCatalog = new TypePluginCatalog(typeof(HttpCloudEventReceiverApi));

                    return typeCatalog;
                });

                services.AddSingleton<IEndpointConfigurationProvider>(customerEndpointConfigurationProvider);
            }

            return services;
        }
    }
}
