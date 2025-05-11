using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Data;

public class NullTagged : ITagged
{
    public static NullTagged Instance { get; } = new NullTagged();
    
    public SidechannelTag Tag
    {
        get => SidechannelTag.InvariantNull;
        set { }
    }
}
