namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class ValidatingDateOnlyCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingDateCollection();
}