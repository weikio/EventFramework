using Microsoft.Extensions.Options;

namespace Weikio.EventFramework.EventCreator
{
    public class DefaultCloudEventCreatorOptionsProvider : ICloudEventCreatorOptionsProvider
    {
        private readonly IOptionsMonitor<CloudEventCreationOptions> _optionsMonitor;

        public DefaultCloudEventCreatorOptionsProvider(IOptionsMonitor<CloudEventCreationOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public CloudEventCreationOptions Get(string optionsName)
        {
            return _optionsMonitor.Get(optionsName);
        }
    }
}
