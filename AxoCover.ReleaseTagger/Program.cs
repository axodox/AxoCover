using AxoCover.Common.Extensions;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AxoCover.ReleaseTagger
{
  class Program
  {
    private const string _owner = "axodox";
    private const string _repository = "AxoCover";
    private static readonly string[] _mergeTargets = new[] { "master", "release" };

    private static readonly Regex _nameRegex = new Regex(@"^(?<branch>.*?)-(?<version>\d+(?:\.\d+)*)$");
    private static readonly Regex _propertyRegex = new Regex(@"#(?<name>\w+):""(?<value>(?:[^""]|(?<=\\)"")*)""");

    static void Main(string[] args)
    {
      var githubToken = Environment.GetEnvironmentVariable("github_token");
      if (githubToken == null)
      {
        Console.WriteLine("Please set the github_token environment variable.");
        return;
      }
      
      TagReleases(githubToken);
    }

    private static void TagReleases(string token)
    {
      Console.WriteLine("AxoCover release tagger");

      try
      {
        Console.Write("Initializing... ");
        var githubClient = new GitHubClient(new ProductHeaderValue($"{_owner}.{_repository}"))
        {
          Credentials = new Credentials(token)
        };
        Console.WriteLine("Done.");

        Console.Write("Collecting release information... ");
        var releases = githubClient.Repository.Release.GetAll(_owner, _repository).Result;
        var latestReleases = releases
          .GroupBy(p => GetBranch(p))
          .Where(p => !_mergeTargets.Contains(p.Key))
          .Select(p => p.OrderBy(q => GetVersion(q)).Last())
          .Where(p => !GetProperties(p).ContainsKey("MergedTo"))
          .ToArray();
        Console.WriteLine("Done.");

        Console.Write("Collecting commit information... ");
        var mergedCommits = _mergeTargets
          .SelectMany(p => githubClient.Repository.Commit.GetAll(_owner, _repository, new CommitRequest() { Sha = p }).Result.Select(q => new { Branch = p, Commit = q }))
          .GroupBy(p => p.Commit.Sha)
          .ToDictionary(p => p.Key, p => p.First().Branch);
        Console.WriteLine("Done.");
        
        foreach (var release in latestReleases)
        {
          if (mergedCommits.TryGetValue(release.TargetCommitish, out var branch))
          {
            try
            {
              Console.Write($"Tagging {release.Name}... ");
              var releaseUpdate = release.ToUpdate();
              if (!string.IsNullOrWhiteSpace(releaseUpdate.Body))
              {
                releaseUpdate.Body += "\r\n";
              }
              releaseUpdate.Body += $"#MergedTo:\"{branch}\"";

              githubClient.Repository.Release.Edit(_owner, _repository, release.Id, releaseUpdate).Wait();
              Console.WriteLine("Done.");
            }
            catch (Exception e)
            {
              Console.WriteLine("Failed!");
              Console.WriteLine(e.GetDescription());
            }
          }
        }
      }
      catch(Exception e)
      {
        Console.WriteLine("Failed!");
        Console.WriteLine(e.GetDescription());
      }
    }

    private static string GetBranch(Release release)
    {
      var nameMatch = _nameRegex.Match(release.Name);
      return nameMatch.Success ? nameMatch.Groups["branch"].Value : string.Empty;
    }

    private static Version GetVersion(Release release)
    {
      var nameMatch = _nameRegex.Match(release.Name);
      var versionString = nameMatch.Success ? nameMatch.Groups["version"].Value : "0";
      while (versionString.Count(p => p == '.') < 3)
      {
        versionString += ".0";
      }

      return Version.Parse(versionString);
    }

    private static Dictionary<string, string> GetProperties(Release release)
    {
      return _propertyRegex
        .Matches(release.Body)
        .OfType<Match>()
        .ToDictionary(p => p.Groups["name"].Value, p => p.Groups["value"].Value);
    }
  }
}
