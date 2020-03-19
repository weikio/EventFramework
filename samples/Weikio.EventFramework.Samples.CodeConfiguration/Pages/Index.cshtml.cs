using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.AspNetCore.Gateways;

namespace Weikio.EventFramework.Samples.CodeConfiguration.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private readonly ICloudEventGatewayManager _cloudEventGatewayManager;
        private readonly HttpGatewayFactory _factory;

        public IndexModel(ILogger<IndexModel> logger, ICloudEventPublisher cloudEventPublisher, 
            ICloudEventGatewayManager cloudEventGatewayManager, HttpGatewayFactory factory)
        {
            _logger = logger;
            _cloudEventPublisher = cloudEventPublisher;
            _cloudEventGatewayManager = cloudEventGatewayManager;
            _factory = factory;
        }

        public CloudEvent CloudEvent { get; set; }

        public void OnGet()
        {
            if (TempData.ContainsKey("el"))
            {
                CloudEvent = JsonSerializer.Deserialize<CloudEvent>(TempData["el"].ToString());
            }
        }

        public async Task<IActionResult> OnPost()
        {
            var publishedEvent = await _cloudEventPublisher.Publish(new CloudEvent("hello_world", new Uri("http://localhost")), "web");

            TempData["el"] = JsonSerializer.Serialize(CloudEvent);

            return RedirectToPage();
        }
        
        public async Task<IActionResult> OnPostCustomer()
        {
            var customerCreated = new CustomerCreated(){FirstName = "Mikael", LastName = "Koskinen"};
            var publishedEvent = await _cloudEventPublisher.Publish(new CloudEvent<CustomerCreated>(customerCreated, new Uri("http://localhost")), "local");

            TempData["el"] = JsonSerializer.Serialize(publishedEvent);

            return RedirectToPage();
        }
        
        public async Task<IActionResult> OnPostSecond()
        {
            var publishedEvent = await _cloudEventPublisher.Publish(new CloudEvent("hello_world", new Uri("http://localhost")), "local2");

            TempData["el"] = JsonSerializer.Serialize(CloudEvent);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostcreateChannel()
        {
            var gateway = _factory.Create("priority", "/api/prioEvents");
            _cloudEventGatewayManager.Add("priority", gateway);

            await _cloudEventGatewayManager.Update();

            return RedirectToPage();
        }
    }
}
