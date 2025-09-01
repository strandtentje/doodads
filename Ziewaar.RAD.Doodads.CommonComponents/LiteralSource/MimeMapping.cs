#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

public static class MimeMapping
{
    public class MimeTypeInfo
    {
        public string MimeType { get; }
        public bool IsText { get; }

        public MimeTypeInfo(string mimeType, bool isText)
        {
            MimeType = mimeType;
            IsText = isText;
        }
    }

    private static readonly Dictionary<string, MimeTypeInfo> ExtensionToMimeType =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Text
            ["txt"] = new("text/plain", true),
            ["csv"] = new("text/csv", true),
            ["tsv"] = new("text/tab-separated-values", true),
            ["log"] = new("text/plain", true),
            ["xml"] = new("application/xml", true),
            ["html"] = new("text/html", true),
            ["htm"] = new("text/html", true),
            ["css"] = new("text/css", true),
            ["js"] = new("application/javascript", true),
            ["mjs"] = new("text/javascript", true),
            ["json"] = new("application/json", true),

            // Images (binary)
            ["png"] = new("image/png", false),
            ["jpg"] = new("image/jpeg", false),
            ["jpeg"] = new("image/jpeg", false),
            ["gif"] = new("image/gif", false),
            ["bmp"] = new("image/bmp", false),
            ["webp"] = new("image/webp", false),
            ["svg"] = new("image/svg+xml", true), // SVG is XML-based = text
            ["ico"] = new("image/x-icon", false),
            ["tif"] = new("image/tiff", false),
            ["tiff"] = new("image/tiff", false),
            ["avif"] = new("image/avif", false),
            ["jfif"] = new("image/jpeg", false),

            // Audio (binary)
            ["mp3"] = new("audio/mpeg", false),
            ["wav"] = new("audio/wav", false),
            ["ogg"] = new("audio/ogg", false),
            ["oga"] = new("audio/ogg", false),
            ["m4a"] = new("audio/mp4", false),
            ["flac"] = new("audio/flac", false),
            ["aac"] = new("audio/aac", false),
            ["opus"] = new("audio/opus", false),

            // Video (binary)
            ["mp4"] = new("video/mp4", false),
            ["m4v"] = new("video/mp4", false),
            ["webm"] = new("video/webm", false),
            ["ogv"] = new("video/ogg", false),
            ["avi"] = new("video/x-msvideo", false),
            ["mov"] = new("video/quicktime", false),
            ["wmv"] = new("video/x-ms-wmv", false),
            ["flv"] = new("video/x-flv", false),
            ["mkv"] = new("video/x-matroska", false),

            // Fonts (binary)
            ["ttf"] = new("font/ttf", false),
            ["otf"] = new("font/otf", false),
            ["woff"] = new("font/woff", false),
            ["woff2"] = new("font/woff2", false),

            // Archives (binary)
            ["zip"] = new("application/zip", false),
            ["tar"] = new("application/x-tar", false),
            ["gz"] = new("application/gzip", false),
            ["rar"] = new("application/vnd.rar", false),
            ["7z"] = new("application/x-7z-compressed", false),
            ["bz2"] = new("application/x-bzip2", false),
            ["xz"] = new("application/x-xz", false),

            // Documents
            ["pdf"] = new("application/pdf", false),
            ["doc"] = new("application/msword", false),
            ["docx"] = new("application/vnd.openxmlformats-officedocument.wordprocessingml.document", false),
            ["xls"] = new("application/vnd.ms-excel", false),
            ["xlsx"] = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", false),
            ["ppt"] = new("application/vnd.ms-powerpoint", false),
            ["pptx"] = new("application/vnd.openxmlformats-officedocument.presentationml.presentation", false),
            ["rtf"] = new("application/rtf", true),
            ["odt"] = new("application/vnd.oasis.opendocument.text", true),
            ["ods"] = new("application/vnd.oasis.opendocument.spreadsheet", false),

            // Code (text)
            ["c"] = new("text/plain", true),
            ["cpp"] = new("text/plain", true),
            ["h"] = new("text/plain", true),
            ["cs"] = new("text/plain", true),
            ["java"] = new("text/plain", true),
            ["py"] = new("text/plain", true),
            ["sh"] = new("text/x-shellscript", true),
            ["bat"] = new("application/x-msdownload", true),
            ["ps1"] = new("text/plain", true),
            ["go"] = new("text/plain", true),
            ["rs"] = new("text/plain", true),
            ["ts"] = new("application/typescript", true),

            // Binaries
            ["exe"] = new("application/vnd.microsoft.portable-executable", false),
            ["dll"] = new("application/vnd.microsoft.portable-executable", false),
            ["bin"] = new("application/octet-stream", false),
            ["msi"] = new("application/x-msdownload", false),

            // Misc
            ["swf"] = new("application/x-shockwave-flash", false),
            ["eot"] = new("application/vnd.ms-fontobject", false),
            ["wasm"] = new("application/wasm", false)
        };

    public static string GetMimeType(FileInfo file) =>
        GetMimeInfo(file).MimeType;

    public static string GetMimeType(string path) =>
        GetMimeInfo(path).MimeType;

    public static bool IsText(FileInfo file) =>
        GetMimeInfo(file).IsText;

    public static bool IsText(string path) =>
        GetMimeInfo(path).IsText;

    public static MimeTypeInfo GetMimeInfo(FileInfo file)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));

        return GetMimeInfo(file.Name);
    }

    public static MimeTypeInfo GetMimeInfo(string filePath)
    {
        if (filePath == null)
            throw new ArgumentNullException(nameof(filePath));

        string ext = Path.GetExtension(filePath)?.TrimStart('.') ?? "";

        return ExtensionToMimeType.TryGetValue(ext.ToLower(), out var info)
            ? info
            : new MimeTypeInfo("application/octet-stream", false);
    }
}