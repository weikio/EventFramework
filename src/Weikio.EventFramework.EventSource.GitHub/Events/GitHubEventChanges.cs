using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Weikio.EventFramework.EventSource.GitHub.Events{

public sealed class GitHubEventChanges
{
    public GitHubEventIssue NewIssue { get; set; }
    public GitHubEventRepository NewRepository { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}
}
