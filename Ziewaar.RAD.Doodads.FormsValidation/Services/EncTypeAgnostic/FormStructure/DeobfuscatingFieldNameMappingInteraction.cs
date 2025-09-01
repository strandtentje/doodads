namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;
public class DeobfuscatingFieldNameMappingInteraction(IInteraction stack, FormStructureInteraction structure, ICsrfFields fields) : IFieldNameMappingInteraction
{
    public IInteraction Stack => stack;
    public object Register => stack.Register;
    public IReadOnlyDictionary<string, object> Memory => stack.Memory;
    public bool TryGetRealNameOf(string incomingName, out string? realName) => fields.TryDeobfuscating(structure.GetName(),  incomingName, out realName);
}