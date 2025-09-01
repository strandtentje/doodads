namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;

public interface IFieldNameMappingInteraction : IInteraction
{
    bool TryGetRealNameOf(string incomingName, out string? realName);
}