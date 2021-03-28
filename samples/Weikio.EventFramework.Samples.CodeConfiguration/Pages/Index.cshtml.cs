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
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.EventSourceWrapping;

namespace Weikio.EventFramework.Samples.CodeConfiguration.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private readonly ICloudEventGatewayManager _cloudEventGatewayManager;
        private readonly IEventSourceInstanceManager _eventSourceInstanceManager;
        private readonly IEventSourceDefinitionProvider _eventSourceDefinitionProvider;

        public IndexModel(ILogger<IndexModel> logger, ICloudEventPublisher cloudEventPublisher, 
            ICloudEventGatewayManager cloudEventGatewayManager, IEventSourceInstanceManager eventSourceInstanceManager, IEventSourceDefinitionProvider eventSourceDefinitionProvider)
        {
            _logger = logger;
            _cloudEventPublisher = cloudEventPublisher;
            _cloudEventGatewayManager = cloudEventGatewayManager;
            _eventSourceInstanceManager = eventSourceInstanceManager;
            _eventSourceDefinitionProvider = eventSourceDefinitionProvider;
        }

        public CloudEvent CloudEvent { get; set; }

        public List<EventSourceDefinition> EventSourceDefinitions { get; set; } = new List<EventSourceDefinition>();
        public static List<CloudEvent> ReceivedEvents { get; set; } = new List<CloudEvent>();

        public void OnGet()
        {
            if (TempData.ContainsKey("el"))
            {
                CloudEvent = JsonSerializer.Deserialize<CloudEvent>(TempData["el"].ToString());
            }

            EventSourceDefinitions = _eventSourceDefinitionProvider.List();
        }

        public async Task<IActionResult> OnPost()
        {
            var publishedEvent = await _cloudEventPublisher.Publish(new CloudEvent("hello_world", new Uri("http://localhost")));

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
        
        public async Task<IActionResult> OnPostCreateInstance()
        {
            // var esId = await _eventSourceInstanceManager.Create(new EventSourceInstanceOptions()
            // {
            //     EventSourceDefinition = "FileEventSource",
            //     Configuration = new FileEventSourceConfiguration() { Filter = "*.txt", Folder = @"c:\temp\listen" },
            //     Autostart = true
            // });

            return RedirectToPage();
        }
    }
}
