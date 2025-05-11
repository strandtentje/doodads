namespace Ziewaar.RAD.Doodads.CoreLibrary.Data;

public enum SidechannelState
{
    /// <summary>
    /// Means the accompanying object should always be updated
    /// </summary>
    Always,
    /// <summary>
    /// Means the accompanying object will null-sink whatever happens.
    /// </summary>
    Never,
    /// <summary>
    /// Means update when stamp is different
    /// </summary>
    StampDifferent,
    /// <summary>
    /// Update when stamp is lower
    /// </summary>
    StampLower,
    /// <summary>
    /// Update when stamp is greater
    /// </summary>
    StampGreater,
}
