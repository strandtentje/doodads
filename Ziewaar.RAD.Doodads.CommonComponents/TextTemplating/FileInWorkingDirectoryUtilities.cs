#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

public static class FileInWorkingDirectoryUtilities
{
    public static object ToStringOrDatedFile(object value)
    {
        if (value is string textValue)
            return textValue;
        else if (value is decimal numValue)
            return numValue;
        else
        {
            var stringified = value.ToString();
            return File.Exists(stringified) ? $"{stringified}#{File.GetLastWriteTime(stringified):O}" : stringified;
        }
    }
}