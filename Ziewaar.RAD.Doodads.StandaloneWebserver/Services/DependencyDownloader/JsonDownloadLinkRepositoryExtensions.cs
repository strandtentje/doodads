namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.DependencyDownloader
{
    public static class JsonDownloadLinkRepositoryExtensions
    {
        public static IEnumerable<string> GetGithubDownloads(this JsonDownloadLinkRepository linkRepo, string owner, string repo) =>
            linkRepo.GetDownloadLinks($"{owner}/{repo}");
    }
}
