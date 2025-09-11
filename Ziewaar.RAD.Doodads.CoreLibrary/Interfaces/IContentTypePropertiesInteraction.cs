namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
public interface IContentTypePropertiesInteraction : IInteraction
{
    IReadOnlyDictionary<string, string> ContentTypeProperties { get; }
}