using TtLog;

namespace Ziewaar.Web.Uploadserver;

public class CommonUploadSizeGovernor : IUploadSizeGovernor
{
    public const long KB = 1024, MB = KB * 1024, GB = MB * 1024, TB = GB * 1024;
    public long HardLimit = 8L * 1024L * 1024L, DiskSpaceLimit = 16L * 1024L * 1024L * 1024L;

    public CommonUploadSizeGovernor()
    {

    }

    public CommonUploadSizeGovernor(long hardLimit, long diskSpaceLimit)
    {
        this.HardLimit = hardLimit;
        this.DiskSpaceLimit = diskSpaceLimit;
    }
    private static readonly Log NoDrive = Log.Oops(
        "Failed to find existing drive for {path}");
    public bool ValidateContentLength(string directory, long contentLength)
    {
        if (contentLength > HardLimit)
            return false;
        var targetDir = new DirectoryInfo(directory);

        var allDrives = DriveInfo.GetDrives();

        foreach (var drive in allDrives)
        {
            if (drive.RootDirectory.FullName == targetDir.Root.FullName)
            {
                // *2 because the multipart response gets stored and pried apart
                // another one for margin.
                var spaceAfterUpload =
                    drive.AvailableFreeSpace - contentLength * 3;
                return spaceAfterUpload > DiskSpaceLimit;
            }
        }

        Log.Post(NoDrive, directory);
        return false;
    }
}