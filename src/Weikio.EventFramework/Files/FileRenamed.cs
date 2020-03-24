namespace Weikio.EventFramework.Files
{
    public class FileRenamed
    {
        public string Name { get; }
        public string FullPath { get; }
        public string OldName { get; }
        public string OldFullPath { get; }

        public FileRenamed(string name, string fullPath, string oldName, string oldFullPath)
        {
            Name = name;
            FullPath = fullPath;
            OldName = oldName;
            OldFullPath = oldFullPath;
        }
    }
}