using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class HelloWorld2
    {
        private readonly ILogger<HelloWorld2> _logger;
        public int State { get; set; }
        
        public string Folder { get; set; }

        public HelloWorld2(ILogger<HelloWorld2> logger)
        {
            _logger = logger;
        }

        public Task<List<CloudEvent>> Execute()
        {
            State += 1;
            _logger.LogInformation("Hello world! " + State + " " + Folder);

            var res = new List<CloudEvent>();

            if ((decimal)State % 3 == 0)
            {
                var ev = new CloudEvent(CloudEventsSpecVersion.V1_0, "hello", new Uri("https://localhost"));
                res.Add(ev);
            }
            return Task.FromResult(res);
        }
    }
}
