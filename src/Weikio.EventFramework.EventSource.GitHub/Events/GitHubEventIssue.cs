using Newtonsoft.Json.Linq;

namespace Weikio.EventFramework.EventSource.GitHub.Events{

public sealed class GitHubEventIssue : GitHubEventIssueOrPullRequest
{
    public string RepositoryUrl { get; set; }
    public string LabelsUrl { get; set; }
    public string EventsUrl { get; set; }
    public JObject PerformedViaGithubApp { get; set; }
}
}
