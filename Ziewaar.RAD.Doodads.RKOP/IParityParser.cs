namespace Ziewaar.RAD.Doodads.RKOP;

public interface IParityParser
{
    ParityParsingState UpdateFrom(ref CursorText text);
}
