using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using TagLib;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

namespace Ziewaar.RAD.Doodads.Multimedia.Tags;

public class TagMemory(Tag tag) : IReadOnlyDictionary<string, object>
{
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => 
        Enum.
            GetNames<MusicTagProperties>().
            Select(tagName => new KeyValuePair<string, object>(tagName, this[tagName])).
            GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => Enum.GetNames<MusicTagProperties>().Length;

    public bool ContainsKey(string key) => 
        Enum.TryParse<MusicTagProperties>(key, ignoreCase: true, out var mtp) && 
        Enum.IsDefined(mtp);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
    {
        value = null;
        if (!Enum.TryParse<MusicTagProperties>(key, ignoreCase: true, out var r))
            return false;
        value = r switch
        {
            MusicTagProperties.Title => tag.Title,
            MusicTagProperties.Subtitle => tag.Subtitle,
            MusicTagProperties.PerformersArray => tag.Performers,
            MusicTagProperties.AlbumArtistsArray => tag.AlbumArtists,
            MusicTagProperties.Album => tag.Album,
            MusicTagProperties.GenresArray => tag.Genres,
            MusicTagProperties.Year => tag.Year,
            MusicTagProperties.Track => tag.Track,
            MusicTagProperties.TrackCount => tag.TrackCount,
            MusicTagProperties.Disc => tag.Disc,
            MusicTagProperties.DiscCount => tag.DiscCount,
            
            MusicTagProperties.Performers => string.Join(", ", tag.Performers),
            MusicTagProperties.AlbumArtists => string.Join(", ", tag.AlbumArtists),
            MusicTagProperties.Genres => string.Join(", ", tag.Genres),
            MusicTagProperties.SearchWords => GetSearchWords(),
            _ => null,
        };
        return value != null;
    }

    private string[] GetSearchWords()
    {
        return tag.Performers.Concat(tag.AlbumArtists).Concat([
            tag.Title, tag.Subtitle, tag.Album, tag.Year.ToString(),
        ]).OfType<string>().SelectMany(x => x.SplitAtNonAlpha()).Distinct().
            Select(x => x.RemoveDiacritics()).Order().ToArray();
    }
    

    public object this[string key] => TryGetValue(key, out var v) ? v : throw new KeyNotFoundException();

    public IEnumerable<string> Keys => Enum.GetNames<MusicTagProperties>();
    public IEnumerable<object> Values => Enum.GetNames<MusicTagProperties>().Select(x => this[x]);
}