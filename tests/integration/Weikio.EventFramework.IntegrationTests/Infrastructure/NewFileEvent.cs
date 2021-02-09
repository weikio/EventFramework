namespace Weikio.EventFramework.IntegrationTests.Infrastructure
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