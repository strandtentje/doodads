namespace Ziewaar.Web.Uploadserver.Extensions;

public static class UploadBoxExtensions
{
    public static readonly string[] Capacities = new string[] {
        "b", "Kb", "Mb", "Gb",
    };
    public static string GetNormalizedSpeed(
        this UploadBox ub)
    {
        var bps = ub.UploadBytesPerSecond;
        int speedIndex = 0;
        while (bps > 1024 && speedIndex < Capacities.Length - 2)
        {
            bps /= 1024;
            speedIndex++;
        }
        return string.Format("{0}{1}", bps, Capacities[speedIndex]);
    }
    public static string GetNormalizedSize(
        this UploadBox ub)
    {
        var size = ub.ContentLengthProgress;
        int sizeIndex = 0;
        while (size > 1024 && sizeIndex < Capacities.Length - 2)
        {
            size /= 1024;
            sizeIndex++;
        }
        return string.Format("{0}{1}", size, Capacities[sizeIndex]);
    }
}