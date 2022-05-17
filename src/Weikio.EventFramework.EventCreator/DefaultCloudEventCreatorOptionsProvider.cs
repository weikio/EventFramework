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
            var result = _optionsMonitor.Get(optionsName);

            if (result != null)
            {
                return result;
            }

            return new CloudEventCreationOptions();
        }
    }
}
