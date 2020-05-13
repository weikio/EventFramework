namespace Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure
{
    public class DeletedFileEvent
    {
        public string FileName { get; }

        public DeletedFileEvent(string fileName)
        {
            FileName = fileName;
        }
    }
}