using System;
using System.IO;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventSource.Files
{
    public class FileCloudEventSourceConfiguration
    {
        /// <summary>
        /// Get or sets if handled events should be archived. Defaults to true
        /// </summary>
        public bool Archive { get; set; } = true;

        /// <summary>
        /// Get or sets the func for getting archive folder
        /// </summary>
        public Func<string, FileCloudEventSourceConfiguration, CloudEvent, string> GetArchiveFolder { get; set; } = (prefix, config, ev) =>
        {
            var archiveFolder = Path.Combine(config.GetRootFolder(prefix), "archive", DateTime.Now.ToString("yyyyMMdd"));

            return archiveFolder;
        };

        /// <summary>
        /// Get or sets the root folder for archive and processing folders 
        /// </summary>
        public Func<string, string> GetRootFolder { get; set; } = prefix =>
        {
            var result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CloudEvents", prefix);

            return result;
        };

        /// <summary>
        /// Get or sets the func for getting folder where received events are processed
        /// </summary>
        public Func<string, FileCloudEventSourceConfiguration, string> GetProcessFolder { get; set; } = (prefix, configuration) =>
        {
            var errorFolder = Path.Combine(configuration.GetRootFolder(prefix), "processing");

            return errorFolder;
        };

        /// <summary>
        /// Get or sets the folder where invalid events are transferred to
        /// </summary>
        public Func<string, FileCloudEventSourceConfiguration, string> GetErrorFolder { get; set; } = (prefix, config) =>
        {
            var errorFolder = Path.Combine(config.GetRootFolder(prefix), "errors", DateTime.Now.ToString("yyyyMMdd"));

            return errorFolder;
        };

        /// <summary>
        /// Get or set the folder to monitor
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// Get or set file monitoring filter
        /// </summary>
        public string Filter { get; set; } = "";

        /// <summary>
        /// Get or set if subfolders should be monitored for changes
        /// </summary>
        public bool IncludeSubfolders { get; set; }
    }
}
