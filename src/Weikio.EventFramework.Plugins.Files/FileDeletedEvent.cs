namespace Weikio.EventFramework.Files
{
    public class FileDeletedEvent
    {
        public string Name { get; }
        public string FullPath { get; }

        public FileDeletedEvent(string name, string fullPath)
        {
            Name = name;
            FullPath = fullPath;
        }
    }
}