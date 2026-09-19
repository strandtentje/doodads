using TagLib;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class FileMetadataInteraction(IInteraction parent, string path, Tag fileTag) : IInteraction
{
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public Tag Tag => fileTag;
    public string Path => path;
    public IReadOnlyDictionary<string, object> Memory => field ??= new FileMetadataDictionary(fileTag);
}