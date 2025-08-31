namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class ValidatingFileCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingFileCollection();
}