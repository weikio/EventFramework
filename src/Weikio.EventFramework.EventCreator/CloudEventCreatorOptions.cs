using System.Collections.Generic;
using System.Linq;

namespace Weikio.EventFramework.EventCreator
{
    public class CloudEventCreatorOptions : ICloudEventCreatorOptionsProvider
    {
        private readonly Dictionary<string, CloudEventCreationOptions> _optionsDictionary;

        public CloudEventCreatorOptions(CloudEventCreationOptions options) : this(new Dictionary<string, CloudEventCreationOptions>() { { "", options } })
        {
        }

        public CloudEventCreatorOptions(Dictionary<string, CloudEventCreationOptions> optionsDictionary)
        {
            if (optionsDictionary == null)
            {
                optionsDictionary = new Dictionary<string, CloudEventCreationOptions>();
            }

            if (optionsDictionary.Count > 0)
            {
                var first = optionsDictionary.First();

                if (first.Value == null)
                {
                    optionsDictionary[first.Key] = default;
                }
            }

            _optionsDictionary = optionsDictionary;
        }

        public CloudEventCreationOptions Get(string optionsName)
        {
            if (_optionsDictionary.ContainsKey(optionsName))
            {
                return _optionsDictionary[optionsName];
            }

            if (_optionsDictionary.ContainsKey(""))
            {
                return _optionsDictionary[""];
            }

            return default;
        }
    }
}
