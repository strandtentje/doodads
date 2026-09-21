using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Ejije.Logging;
using ImageMagick;
using TagLib;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.TtLog.Utilities;
using File = TagLib.File;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class PictureMetadata : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForInteraction(interaction, out FileMetadataInteraction fmi);
        
        var hash =
            Convert.ToHexString(
                MD5.HashData(Encoding.UTF8.GetBytes(fmi.Tag.Album + string.Concat(fmi.Tag.Performers))));
        
        RepeatToRegister(constants, interaction,
            EnumerateBase64Thumbnails(fmi.Tag, fmi.Path, hash),
            elseOnEmpty: true);
    }

    private IEnumerable<object> EnumerateBase64Thumbnails(Tag outputTag, string originalFile, string hash)
    {
        var path = Path.Combine(SpecialPaths.AppDataForAssy, "albumart-cache");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        int i = 0;
        for (; i < outputTag.Pictures.Length; i++)
        {
            var file = GetCacheFile(path, hash, i);
            var x = i;
            yield return Make64UrlImage(() => (outputTag.Pictures[x].Data.Data, outputTag.Pictures[x].Data.Count), file);
        }

        var parent = Path.GetDirectoryName(originalFile);
        if (parent == null) yield break;
        if (Directory.EnumerateFiles(parent).Take(40).Count() >= 40) yield break;
        
        var pictures = Directory.EnumerateFiles(parent, "*.jpg").Concat(Directory.EnumerateFiles(parent, "*.jpeg")).Concat(Directory.EnumerateFiles(parent, "*.png")).Take(3);
        foreach (var looseFile in pictures)
        {
            var x = i++;
            var file = GetCacheFile(path, hash, x);
            var data = System.IO.File.ReadAllBytes(looseFile);
            yield return Make64UrlImage(() => (data, data.Length), file);
        }
    }

    private static readonly Log InvalidImage = Log.Oops("Invalid image {exception}");

    private static string GetCacheFile(string path, string albHash, int ix) => Path.Combine(path, $"{albHash}-{ix}.jpg");

    public string Make64UrlImage(Func<(byte[] d, int l)> getData, string cacheFile)
    {
        try
        {
            if (!System.IO.File.Exists(cacheFile))
            {
                (var data, var length) = getData();
                using var img = new ImageMagick.MagickImage(new Span<byte>(data, 0, length));
                img.Scale(96, 96);
                var bw = new ArrayBufferWriter<byte>();
                img.Quality = 50;
                img.Write(bw, MagickFormat.Jpg);
                var result = string.Concat("data:image/jpeg;charset=utf-8;base64,", Convert.ToBase64String(bw.WrittenSpan));
                System.IO.File.WriteAllText(cacheFile, result);
                return result;
            }
            else
            {
                return System.IO.File.ReadAllText(cacheFile);
            }
        }
        catch (Exception ex)
        {
            Log.Post(InvalidImage, ex);
            return "";
        }
    }
}