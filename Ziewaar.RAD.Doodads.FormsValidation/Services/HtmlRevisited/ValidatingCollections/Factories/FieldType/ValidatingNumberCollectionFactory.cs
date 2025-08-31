namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class ValidatingNumberCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingNumberCollection();
}