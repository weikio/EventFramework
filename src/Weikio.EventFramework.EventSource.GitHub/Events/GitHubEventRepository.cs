﻿using System;
using Newtonsoft.Json.Linq;

namespace Weikio.EventFramework.EventSource.GitHub.Events{

public sealed class GitHubEventRepository
{
    public int Id { get; set; }
    public string NodeId { get; set; }
    public string Name { get; set; }
    public string FullName { get; set; }
    public bool Private { get; set; }
    public GitHubEventUser Owner { get; set; }
    public string HtmlUrl { get; set; }
    public string Description { get; set; }
    public bool Fork { get; set; }
    public string Url { get; set; }
    public string ForksUrl { get; set; }
    public string KeysUrl { get; set; }
    public string CollaboratorsUrl { get; set; }
    public string TeamsUrl { get; set; }
    public string HooksUrl { get; set; }
    public string IssueEventsUrl { get; set; }
    public string EventsUrl { get; set; }
    public string AssigneesUrl { get; set; }
    public string BranchesUrl { get; set; }
    public string TagsUrl { get; set; }
    public string BlobsUrl { get; set; }
    public string GitTagsUrl { get; set; }
    public string GitRefsUrl { get; set; }
    public string TreesUrl { get; set; }
    public string StatusesUrl { get; set; }
    public string LanguagesUrl { get; set; }
    public string StargazersUrl { get; set; }
    public string ContributorsUrl { get; set; }
    public string SubscribersUrl { get; set; }
    public string SubscriptionUrl { get; set; }
    public string CommitsUrl { get; set; }
    public string GitCommitsUrl { get; set; }
    public string CommentsUrl { get; set; }
    public string IssueCommentUrl { get; set; }
    public string ContentsUrl { get; set; }
    public string CompareUrl { get; set; }
    public string MergesUrl { get; set; }
    public string ArchiveUrl { get; set; }
    public string DownloadsUrl { get; set; }
    public string IssuesUrl { get; set; }
    public string PullsUrl { get; set; }
    public string MilestonesUrl { get; set; }
    public string NotificationsUrl { get; set; }
    public string LabelsUrl { get; set; }
    public string ReleasesUrl { get; set; }
    public string DeploymentsUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PushedAt { get; set; }
    public string GitUrl { get; set; }
    public string SshUrl { get; set; }
    public string CloneUrl { get; set; }
    public string SvnUrl { get; set; }
    public string Homepage { get; set; }
    public int Size { get; set; }
    public int StargazersCount { get; set; }
    public int WatchersCount { get; set; }
    public string Language { get; set; }
    public bool HasIssues { get; set; }
    public bool HasProjects { get; set; }
    public bool HasDownloads { get; set; }
    public bool HasWiki { get; set; }
    public bool HasPages { get; set; }
    public int ForksCount { get; set; }
    public string MirrorUrl { get; set; }
    public bool Archived { get; set; }
    public bool Disabled { get; set; }
    public int OpenIssuesCount { get; set; }
    public JObject License { get; set; }
    public int Forks { get; set; }
    public int OpenIssues { get; set; }
    public int Watchers { get; set; }
    public string DefaultBranch { get; set; }
    public bool AllowSquashMerge { get; set; }
    public bool AllowMergeCommit { get; set; }
    public bool AllowRebaseMerge { get; set; }
    public bool DeleteBranchOnMerge { get; set; }
}
}
