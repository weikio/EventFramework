namespace Weikio.EventFramework.EventSource.Files
{
    public class FileSystemEventSourceConfiguration
    {
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
