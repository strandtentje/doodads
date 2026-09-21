using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.TtLog.Utilities;

namespace Ziewaar.RAD.Doodads.Multimedia;

public enum FileMetadataKeys
{
    Length,
    Album,
    Title,
    Track,
    Year,
    TrackCount
}

public class MetadataKeywords : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForInteraction(interaction, out FileMetadataInteraction fmi);
        string[] primaryParts = [fmi.Tag.Album, fmi.Tag.Title, fmi.Tag.Year.ToString()];
        var combinedParts = primaryParts.Concat(fmi.Tag.Genres).Concat(fmi.Tag.Performers);

        IEnumerable<string> expandedParts = combinedParts.SelectMany(x => x.MakeWrappable()).Enshortify()
            .Select(x => x.RemoveDiacritics())
            .Select(uncleanString => uncleanString.ToLower().Where(char.IsLetterOrDigit))
            .Select(x => new string(x.ToArray())).Enshortify();

    }
}