namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

public interface ILongPosition
{
    long Limit { get; }
    long Cursor { get; }
}