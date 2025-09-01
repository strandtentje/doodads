namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;
public class FormStructureMember(string name, IValidatingCollectionFactory validatorFactory)
{
    public static FormStructureMemberBuilder Builder => new();
    public string Name => name;
    public IValidatingCollection CreateValidatingCollection() => validatorFactory.Create();
}