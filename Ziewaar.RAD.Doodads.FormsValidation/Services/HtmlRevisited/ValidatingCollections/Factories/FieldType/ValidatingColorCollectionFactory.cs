namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class ValidatingColorCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingColorCollection();
}