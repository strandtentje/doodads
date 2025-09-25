#nullable enable
namespace Ziewaar.RAD.Doodads.EnumerableStreaming
{
#pragma warning disable 67
    public static class SignatureMimeGuesser
    {
        public static string GuessMimeType(List<byte> bytes, out bool isText)
        {
            isText = false;


            if (bytes.Count >= 4)
            {
                if (bytes[0] == 0x89 && bytes[1] == (byte)'P' && bytes[2] == (byte)'N' && bytes[3] == (byte)'G')
                    return "image/png";
                if (bytes[0] == 0xFF && bytes[1] == 0xD8)
                    return "image/jpeg";
                if (bytes[0] == (byte)'G' && bytes[1] == (byte)'I' && bytes[2] == (byte)'F')
                    return "image/gif";
                if (bytes[0] == 0x42 && bytes[1] == 0x4D)
                    return "image/bmp";
                if ((bytes[0] == 0x49 && bytes[1] == 0x49) || (bytes[0] == 0x4D && bytes[1] == 0x4D))
                    return "image/tiff";
                if (bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46)
                    return "application/pdf";
                if (bytes[0] == 0xD0 && bytes[1] == 0xCF && bytes[2] == 0x11 && bytes[3] == 0xE0)
                    return "application/vnd.ms-office";
                if (bytes[0] == 0x50 && bytes[1] == 0x4B && bytes[2] == 0x03 && bytes[3] == 0x04)
                {
                    if (FindString(bytes, "[Content_Types].xml"))
                    {
                        if (FindString(bytes, "word/"))
                            return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        if (FindString(bytes, "xl/"))
                            return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        if (FindString(bytes, "ppt/"))
                            return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    }

                    return "application/zip";
                }

                if (bytes[0] == 0x1F && bytes[1] == 0x8B)
                    return "application/gzip";
                if (bytes[0] == 0x52 && bytes[1] == 0x61 && bytes[2] == 0x72 && bytes[3] == 0x21)
                    return "application/vnd.rar";
                if (bytes[0] == 0x7F && bytes[1] == (byte)'E' && bytes[2] == (byte)'L' && bytes[3] == (byte)'F')
                    return "application/x-elf";
                if (bytes[0] == 0x4D && bytes[1] == 0x5A)
                    return "application/vnd.microsoft.portable-executable";
                if (FindString(bytes, "<svg"))
                    return "image/svg+xml";
                if (FindString(bytes, "<html") || FindString(bytes, "<!DOCTYPE html"))
                    return "text/html";
                if (FindString(bytes, "<?xml"))
                    return "application/xml";
                if (FindString(bytes, "{") && FindString(bytes, ":"))
                    return "application/json";
                if (FindString(bytes, "function") || FindString(bytes, "var ") || FindString(bytes, "const "))
                    return "application/javascript";


// Audio/Video detection
                if (bytes.Count >= 12)
                {
                    if (Encoding.ASCII.GetString(bytes.GetRange(4, 4).ToArray()) == "ftyp")
                    {
                        var brand = Encoding.ASCII.GetString(bytes.GetRange(8, 4).ToArray());
                        if (brand.StartsWith("mp4")) return "video/mp4";
                        if (brand.StartsWith("M4A")) return "audio/mp4";
                    }

                    if (bytes[0] == 0x4F && bytes[1] == 0x67 && bytes[2] == 0x67 && bytes[3] == 0x53)
                    {
// Ogg container
                        if (FindString(bytes, "vorbis")) return "audio/ogg";
                        if (FindString(bytes, "theora")) return "video/ogg";
                        return "application/ogg";
                    }

                    if (bytes[0] == 0x1A && bytes[1] == 0x45 && bytes[2] == 0xDF && bytes[3] == 0xA3)
                        return "video/x-matroska";
                    if (bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46)
                    {
                        var format = Encoding.ASCII.GetString(bytes.GetRange(8, 4).ToArray());
                        if (format == "WAVE") return "audio/wav";
                        if (format == "AVI ") return "video/x-msvideo";
                    }

                    if (FindString(bytes, "fLaC")) return "audio/flac";
                    if (FindString(bytes, "OggS")) return "application/ogg";
                    if (FindString(bytes, "ID3")) return "audio/mpeg"; // MP3
                }
            }


            if (LooksLikeText(bytes))
            {
                isText = true;
                return "text/plain";
            }


            return "application/octet-stream";
        }


        private static bool LooksLikeText(List<byte> bytes)
        {
            int printable = 0;
            foreach (var b in bytes)
            {
                if (b == 0) return false;
                if (b >= 32 && b <= 126 || b == 9 || b == 10 || b == 13)
                    printable++;
            }

            return printable > 0.9 * bytes.Count;
        }


        private static bool FindString(List<byte> bytes, string text)
        {
            var needle = Encoding.UTF8.GetBytes(text);
            for (int i = 0; i <= bytes.Count - needle.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < needle.Length; j++)
                {
                    if (bytes[i + j] != needle[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match) return true;
            }

            return false;
        }
    }
}