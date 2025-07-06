#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
public interface IInteraction
{
    IInteraction Stack { get; }
    object Register { get; }
    IReadOnlyDictionary<string, object> Memory { get; }
}
public interface ICsrfTokenSourceInteraction : IInteraction
{
    public ICsrfFields Fields { get; }
}
public interface ICsrfFields
{
    string PackField(string formName, string fieldName);
    bool TryRecoverByTrueName(string formName, string trueName, out string? fieldAlias);
    bool TryRecoverByAlias(string formName, string fieldAlias, out string? trueFieldName);
    string[] GetWhitelist(string formName);
}