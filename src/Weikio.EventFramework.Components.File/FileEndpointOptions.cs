using System;
using System.Net.Http;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Components.File
{
    public class FileEndpointOptions
    {
        /// <summary>
        /// The Folder where to write the event files. <see cref="GetFolder"/> can be used to create more advanced configuration
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// Gets or sets the func for getting the folder where to write the event files. Defaults to <see cref="Folder"/>.
        /// </summary>
        public Func<FileEndpointOptions, CloudEvent, string> GetFolder { get; set; } = (options, ev) =>
        {
            return options.Folder;
        };
        
        /// <summary>
        /// Gets or sets the filename func for the CloudEvent. Defaults to id of event + .json extension.
        /// </summary>
        public Func<FileEndpointOptions, CloudEvent, string> GetFileName { get; set; } = (options, ev) =>
        {
            return $"{ev.Id}.json";
        };
    }
}
