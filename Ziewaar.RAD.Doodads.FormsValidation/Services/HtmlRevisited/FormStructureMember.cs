namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class FormStructureMember(string name, IValidatingCollectionFactory validatorFactory)
{
    public static FormStructureMemberBuilder Builder => new();
    public string Name => name;
    public IValidatingCollection GetValidatingCollection() => validatorFactory.Create();
}