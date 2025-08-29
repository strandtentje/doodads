namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class ValidatingTimeCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingTimeCollection();
}