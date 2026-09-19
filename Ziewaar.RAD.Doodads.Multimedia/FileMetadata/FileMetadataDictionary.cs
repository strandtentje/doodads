using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TagLib;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class FileMetadataDictionary(Tag fileTag) : IReadOnlyDictionary<string, object>
{
    public int Count => Keys.Count();
    public object this[string key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();
    public IEnumerable<string> Keys => KeyLut.Keys;
    public IEnumerable<object> Values => field ??= KeyLut.Keys.Select(x => this[x]);

    private static readonly IReadOnlyDictionary<string, FileMetadataKeys> KeyLut =
        Enum.GetValues(typeof(FileMetadataKeys)).Cast<FileMetadataKeys>()
            .ToDictionary(x => Enum.GetName(typeof(FileMetadataKeys), x)!.ToLower(), x => x,
                StringComparer.OrdinalIgnoreCase);

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
        Keys.Select(x => new KeyValuePair<string, object>(x, this[x])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool ContainsKey(string key) => KeyLut.ContainsKey(key);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
    {
        if (!KeyLut.TryGetValue(key, out var selection))
        {
            value = null;
            return false;
        }

        switch (selection)
        {
            case FileMetadataKeys.Length:
                value = fileTag.Length;
                return true;
            case FileMetadataKeys.Album:
                value = fileTag.Album;
                return true;
            case FileMetadataKeys.Title:
                value = fileTag.Title;
                return true;
            case FileMetadataKeys.Track:
                value = fileTag.Track;
                return true;
            case FileMetadataKeys.Year:
                value = fileTag.Year;
                return true;
            case FileMetadataKeys.TrackCount:
                value = fileTag.TrackCount;
                return true;
            default:
                value = null;
                return false;
        }
    }
}