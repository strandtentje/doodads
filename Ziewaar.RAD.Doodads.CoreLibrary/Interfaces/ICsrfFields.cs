#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface ICsrfFields
{
    string NewObfuscation(string formName, string fieldName);
    bool TryObfuscating(string formName, string trueName, out string? fieldAlias);
    bool TryDeobfuscating(string formName, string fieldAlias, out string? trueFieldName);
    string[] GetObfuscatedWhitelist(string formName);
    string[] GetDeobfuscatedWhitelist(string formName);
    void UnregisterAlias(string formName, string alias);
}