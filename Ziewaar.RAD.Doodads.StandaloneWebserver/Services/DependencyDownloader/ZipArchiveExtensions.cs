using System.IO.Compression;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.DependencyDownloader
{
    public static class ZipArchiveExtensions
    {
        public static string FindCommonParentPath(this ZipArchive archive, params string[] subdirs)
        {
            string commonParentPath;
            List<string>? commonParentPathElements = null;
            foreach (var item in archive.Entries)
            {
                if (!subdirs.Any(item.FullName.StartsWith))
                    continue;
                var pathMembers = item.FullName.
                    Split('/', StringSplitOptions.RemoveEmptyEntries).ToArray();
                if (commonParentPathElements == null)
                {
                    commonParentPathElements = [.. pathMembers];
                }
                else
                {
                    while (commonParentPathElements.Count > pathMembers.Length)
                        commonParentPathElements.RemoveAt(commonParentPathElements.Count - 1);

                    int pathMemberIx;
                    for (pathMemberIx = 0;
                        pathMemberIx < commonParentPathElements.Count &&
                        commonParentPathElements[pathMemberIx] == pathMembers[pathMemberIx];
                        pathMemberIx++) ;

                    while (commonParentPathElements.Count > pathMemberIx)
                        commonParentPathElements.RemoveAt(commonParentPathElements.Count - 1);
                }
            }
            commonParentPath = string.Join("/", commonParentPathElements ?? []);
            return commonParentPath;
        }
    }
}