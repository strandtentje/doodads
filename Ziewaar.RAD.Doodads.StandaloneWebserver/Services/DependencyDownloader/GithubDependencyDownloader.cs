namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.DependencyDownloader
{
    public class GithubDependencyDownloader(
        JsonDownloadLinkRepository downloads, 
        TemporaryDownloader downloader,
        BinExtractor extractor)
    {
        public void Download(string owner, string repo, string lookFor, params string[] onlySubdirs)
        {
            try
            {
                var files = downloads.GetGithubDownloads(owner, repo);
                var bestFileCandidates = files.
                    Where(x => x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)).
                    OrderByDescending(x => x.Contains(lookFor, StringComparison.OrdinalIgnoreCase));
                var bestUrl = bestFileCandidates.First();

                var downloadedFile = downloader.DownloadTemporary(bestUrl);

                extractor.ExtractToBin(downloadedFile, true, onlySubdirs);
            }
            catch (BinaryDependencyRetrievalException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BinaryDependencyRetrievalException(ex, $"{nameof(Download)} from github {owner}/{repo}");
            }
        }
    }
}
