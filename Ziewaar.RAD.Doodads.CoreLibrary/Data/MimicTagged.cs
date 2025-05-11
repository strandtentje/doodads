using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Data;

public class MimicTagged<TMimic>(ITagged original, TMimic mimicObject) : ITaggedData<TMimic>
{
    public SidechannelTag Tag
    {
        get => original.Tag;
        set => original.Tag = value;
    }

    public TMimic Data => mimicObject;
}
