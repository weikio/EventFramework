using Xunit;

namespace Weikio.EventFramework.IntegrationTests.Infrastructure
{
    public class DeletedFileEvent
    {
        public string FileName { get; }

        public DeletedFileEvent(string fileName)
        {
            FileName = fileName;
        }
    }
    
    [CollectionDefinition(nameof(NotThreadSafeResourceCollection), DisableParallelization = true)]
    public class NotThreadSafeResourceCollection
    {
    }
}
