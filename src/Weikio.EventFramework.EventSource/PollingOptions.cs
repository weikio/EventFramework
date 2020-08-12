using System;

namespace Weikio.EventFramework.EventSource
{
    public class PollingOptions
    {
        public TimeSpan? PollingFrequency { get; set; } = TimeSpan.FromSeconds(30);
    }
}
