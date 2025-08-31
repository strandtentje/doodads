namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class ValidatingDateTimeCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingDateTimeCollection();
}