namespace Ziewaar.RAD.Doodads.CoreLibrary.Data;

public class SidechannelTag
{
    public static readonly SidechannelTag InvariantNull = new SidechannelTag()
    {
        Stamp = 0, TaintCondition = SidechannelState.Never,
    };
    public static readonly SidechannelTag Always = new SidechannelTag()
    {
        Stamp = 0, TaintCondition = SidechannelState.Always,
    };
    public static readonly SidechannelTag UpdateWhenChanged = new()
    {
        Stamp = 0, TaintCondition = SidechannelState.StampDifferent,
    };
    public SidechannelState TaintCondition;
    public bool IsTainted;
    public long Stamp;
}
