using Octokit;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DevInsightCli.Services
{
    public class GitHubService
    {
        private readonly GitHubClient _githubClient;

        public GitHubService()
        {
            // Initialize Octokit client.
            // For public repositories, no authentication is strictly needed for read-only operations.
            // For private repositories or to avoid rate limiting, authentication is required.
            // This should be configured properly, e.g., via environment variables or a config file.
            _githubClient = new GitHubClient(new ProductHeaderValue("DevInsightCli"));

            // Example: Using a personal access token (PAT) from an environment variable
            // var pat = Environment.GetEnvironmentVariable("GITHUB_PAT");
            // if (!string.IsNullOrEmpty(pat))
            // {
            //     _githubClient.Credentials = new Credentials(pat);
            // }
        }

        public async Task<string> GetPrDetailsAsync(string prUrl)
        {
            var (owner, repoName, prNumber) = ParsePrUrl(prUrl);

            if (owner == null || repoName == null || prNumber == 0)
            {
                return $"Invalid PR URL format: {prUrl}. Expected format: https://github.com/owner/repo/pull/number";
            }

            try
            {
                var pullRequest = await _githubClient.PullRequest.Get(owner, repoName, prNumber);

                return $"PR Details for: {prUrl}\n" +
                       $"Title: {pullRequest.Title}\n" +
                       $"Author: {pullRequest.User.Login}\n" +
                       $"Number: {pullRequest.Number}\n" +
                       $"State: {pullRequest.State}\n" +
                       $"Created At: {pullRequest.CreatedAt}\n" +
                       $"Description (first 100 chars): {pullRequest.Body?.Substring(0, Math.Min(pullRequest.Body.Length, 100))}...";
            }
            catch (ApiException ex)
            {
                return $"Error fetching PR details from GitHub: {ex.Message} (URL: {prUrl})";
            }
            catch (Exception ex)
            {
                return $"An unexpected error occurred: {ex.Message} (URL: {prUrl})";
            }
        }

        private (string? owner, string? repoName, int prNumber) ParsePrUrl(string prUrl)
        {
            // Regex to parse GitHub PR URL: https://github.com/owner/repo/pull/number
            var regex = new Regex(@"https://github\.com/([^/]+)/([^/]+)/pull/(\d+)", RegexOptions.IgnoreCase);
            var match = regex.Match(prUrl);

            if (match.Success)
            {
                string owner = match.Groups[1].Value;
                string repoName = match.Groups[2].Value;
                if (int.TryParse(match.Groups[3].Value, out int prNumber))
                {
                    return (owner, repoName, prNumber);
                }
            }
            return (null, null, 0);
        }
    }
}
