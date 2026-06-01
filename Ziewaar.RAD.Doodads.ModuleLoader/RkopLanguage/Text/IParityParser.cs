namespace Ziewaar.RAD.Doodads.ModuleLoader.RkopLanguage.Text;

public interface IParityParser
{
    ParityParsingState UpdateFrom(ref CursorText text);
}
