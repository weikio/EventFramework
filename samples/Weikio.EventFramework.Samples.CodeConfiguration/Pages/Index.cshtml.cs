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

namespace Weikio.EventFramework.Samples.CodeConfiguration.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ICloudEventPublisher _cloudEventPublisher;

        public IndexModel(ILogger<IndexModel> logger, ICloudEventPublisher cloudEventPublisher)
        {
            _logger = logger;
            _cloudEventPublisher = cloudEventPublisher;
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
            var publishedEvent = await _cloudEventPublisher.Publish(new CloudEvent("hello_world", new Uri("http://localhost")));

            TempData["el"] = JsonSerializer.Serialize(CloudEvent);

            return RedirectToPage();
        }
    }
}
