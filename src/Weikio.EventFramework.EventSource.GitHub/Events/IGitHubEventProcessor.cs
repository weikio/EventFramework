namespace Weikio.EventFramework.EventSource.GitHub.Events
{
    public interface IGitHubEventProcessor
    {
        void Process(GitHubEvent @event);
    }
}
