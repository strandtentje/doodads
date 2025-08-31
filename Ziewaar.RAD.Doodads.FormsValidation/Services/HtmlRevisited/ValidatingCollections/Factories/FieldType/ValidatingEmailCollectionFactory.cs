namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class ValidatingEmailCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingEmailCollection();
}