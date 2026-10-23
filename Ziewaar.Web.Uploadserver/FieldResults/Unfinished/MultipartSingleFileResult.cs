namespace Ziewaar.Web.Uploadserver.FieldResults.Unfinished;

public class MultipartSingleFileResult : IMultipartComponentResult
{
    public string Fault { get { return null; } }

    public bool IsExpectingContent { get { return true; } }

    public readonly string FieldName, TempPath, FullPath, ContentType;
    private FileStream TempFileHandle;
    private static readonly char[] IllegalFilenameChars = Path.GetInvalidFileNameChars();
    private MultipartSingleFileResult(
        string fieldName,
        string tempPath,
        FileStream tempFileHandle,
        string fullPath,
        string contentType)
    {
        this.FieldName = fieldName;
        this.TempPath = tempPath;
        this.TempFileHandle = tempFileHandle;
        this.FullPath = fullPath;
        this.ContentType = contentType;
    }

    public static bool TryCreateFor(
        string fieldName, string workingDirectory, string fileName,
        string contentType, out MultipartSingleFileResult result)
    {
        var safeFileName = new string(
            fileName.ToCharArray().
                Where(x => !IllegalFilenameChars.Contains(x)).ToArray());
        if (!Directory.Exists(workingDirectory))
            Directory.CreateDirectory(workingDirectory);
        var fullPath = Path.Combine(workingDirectory, safeFileName);
        if (File.Exists(fullPath))
        {
            result = null;
            return false;
        }

        var tempPath = Path.GetTempFileName();
        var fileStream = File.OpenWrite(tempPath);
        result = new MultipartSingleFileResult(
            fieldName, tempPath, fileStream, fullPath, contentType);
        return true;
    }

    public string Name { get { return FieldName; } }

    public void GiveBinaryData(byte[] data, int start, int length)
    {
        this.TempFileHandle.Write(data, start, length);
    }

    public IMultipartFinishedComponentResult Finish()
    {
        this.TempFileHandle.Flush();
        this.TempFileHandle.Close();
        this.TempFileHandle = null;

        File.Move(TempPath, FullPath);
        return new MultipartFinishedSingleFileResult(
            this.Name, this.FullPath);
    }

    public void Dispose()
    {
        if (TempFileHandle != null)
        {
            TempFileHandle.Dispose();
            TempFileHandle = null;
        }
        if (File.Exists(TempPath))
        {
            File.Delete(TempPath);
        }
    }
    public override string ToString()
    {
        return string.Format("MultipartSingleFileResult({0})",
            this.FullPath);
    }
}