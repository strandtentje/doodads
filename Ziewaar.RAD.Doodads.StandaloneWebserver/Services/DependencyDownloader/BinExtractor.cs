using System.IO.Compression;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.DependencyDownloader
{
    public class BinExtractor
    {
        public static string BinLocation => field ??=
            new FileInfo(typeof(JsonDownloadLinkRepository).Assembly.Location).Directory?.FullName ??
            throw new BinaryDependencyRetrievalException(new NullReferenceException(), "No assembly location");
        public void ExtractToBin(string localFile, bool overwrite, params string[] subdirs)
        {
            using (var fileReader = File.OpenRead(localFile))
            {
                string? commonParentPath = null;
                using (var archive = new ZipArchive(fileReader))
                {
                    commonParentPath = archive.FindCommonParentPath(subdirs);
                    foreach (var item in archive.Entries)
                    {
                        if (!subdirs.Any(item.FullName.StartsWith))
                            continue;
                        var extractSuffix = item.FullName;
                        if (extractSuffix.StartsWith(commonParentPath))
                            extractSuffix = extractSuffix.Substring(commonParentPath.Length);
                        extractSuffix = extractSuffix.Trim().Trim('/');
                        if (extractSuffix.Length == 0)
                            continue;                        
                        var newFullPath = Path.Combine(BinLocation, extractSuffix);
                        if (!Directory.Exists(Path.GetDirectoryName(newFullPath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(newFullPath)!);
                        item.ExtractToFile(newFullPath, overwrite);
                    }
                }
            }
            File.Delete(localFile);
        }
    }
}
