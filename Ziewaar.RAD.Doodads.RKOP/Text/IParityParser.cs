namespace Ziewaar.RAD.Doodads.RKOP.Text;

public interface IParityParser
{
    ParityParsingState UpdateFrom(ref CursorText text);
}
