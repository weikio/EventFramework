using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventSource.GitHub
{
    public class GitHubConfiguration : ApiEventSourceConfigurationBase
    {
        public string Secret { get; set; }
        public override string Route { get; set; } = "github";
    }
}
