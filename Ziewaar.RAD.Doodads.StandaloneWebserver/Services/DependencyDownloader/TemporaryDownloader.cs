using static System.Net.WebRequestMethods;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.DependencyDownloader
{
    public class TemporaryDownloader
    {
        public static string TempLocation
        {
            get
            {
                var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var doodadsTemp = Path.Combine(appdata, "doodads", "downloads");
                if (!Directory.Exists(doodadsTemp))
                    Directory.CreateDirectory(doodadsTemp);
                return doodadsTemp;
            }
        }
        public string DownloadTemporary(string remoteUrl)
        {
            try
            {
                var rearTrimmed = remoteUrl.TrimEnd('/').Trim();
                var targetName = rearTrimmed.Substring(rearTrimmed.LastIndexOf('/') + 1);
                var targetPath = Path.Combine(TempLocation, targetName);

                if (System.IO.File.Exists(targetPath)) return targetPath;

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("strandtentje/doodads");
                    httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                    using (var remoteFile = httpClient.GetStreamAsync(remoteUrl).Result)
                    {
                        using (var localFile = System.IO.File.OpenWrite(targetPath))
                        {
                            remoteFile.CopyTo(localFile);
                        }
                    }
                }

                return targetPath;
            }
            catch (Exception e)
            {
                throw new BinaryDependencyRetrievalException(e, $"`{nameof(DownloadTemporary)}` for `{remoteUrl}`" );
            }
        }
    }
}
