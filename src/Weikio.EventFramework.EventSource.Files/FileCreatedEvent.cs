namespace Weikio.EventFramework.EventSource.Files
{
    public class FileCreatedEvent
    {
        public string Name { get; }
        public string FullPath { get; }

        public FileCreatedEvent(string name, string fullPath)
        {
            Name = name;
            FullPath = fullPath;
        }
    }
}