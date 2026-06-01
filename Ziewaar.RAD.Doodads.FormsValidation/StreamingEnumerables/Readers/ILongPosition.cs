namespace Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

public interface ILongPosition
{
    long Limit { get; }
    long Cursor { get; }
}