using System;
using System.Collections.Generic;
using System.Text;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.DependencyDownloader
{
    public class JsonDownloadLinkRepository(string rootUrl, string suffix, string lookForKeyUnquoted)
    {
        public IEnumerable<string> GetDownloadLinks(string infix)
        {
            var lookForKey = $@"""{lookForKeyUnquoted}""";
            var urlToGet = $"{rootUrl}{infix}{suffix}";
            try
            {
                using var http = new HttpClient();
                http.DefaultRequestHeaders.UserAgent.TryParseAdd("strandtentje/doodads");
                http.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                var releasePage = http.GetStringAsync(urlToGet).Result;
                int bduPos = 0;
                List<string> downloadLinks = new List<string>();
                for (bduPos = releasePage.IndexOf(lookForKey, bduPos) + lookForKey.Length;
                    bduPos >= lookForKey.Length && bduPos < releasePage.Length;
                    bduPos = releasePage.IndexOf(lookForKey, bduPos) + lookForKey.Length)
                {
                    var posAfterOpenQuote = releasePage.IndexOf('"', bduPos) + 1;
                    var nextQuotePos = posAfterOpenQuote;
                    do
                    {
                        nextQuotePos = releasePage.IndexOf('"', nextQuotePos);
                    } while (releasePage[nextQuotePos - 1] == '\\');
                    var linkLength = nextQuotePos - posAfterOpenQuote;
                    downloadLinks.Add(releasePage.Substring(posAfterOpenQuote, linkLength));
                }

                return downloadLinks.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw new BinaryDependencyRetrievalException(ex,
                    $"`{nameof(GetDownloadLinks)}` for `{rootUrl}{infix}{suffix}` when finding `{lookForKey}`");
            }
        }
        public static JsonDownloadLinkRepository ForGithub() => new JsonDownloadLinkRepository(
            "https://api.github.com/repos/", "/releases/latest", @"browser_download_url");
    }
}
