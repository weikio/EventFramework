namespace Weikio.EventFramework.Samples.EventSource
{
    public class NewFileEvent
    {
        public string FileName { get; }

        public NewFileEvent(string fileName)
        {
            FileName = fileName;
        }
    }
}
