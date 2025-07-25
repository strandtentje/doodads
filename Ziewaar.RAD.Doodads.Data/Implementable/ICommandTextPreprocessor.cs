#nullable enable
namespace Ziewaar.RAD.Doodads.Data.Implementable;
#pragma warning disable 67
public interface ICommandTextPreprocessor
{
    string[] DetermineParamNames(string queryText);
    string GenerateQueryFor(string fileName);
    string MakeFilenameSpecific(string queryTextOrFilePath);
}