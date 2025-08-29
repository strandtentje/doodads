namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class ValidatingWeekCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingWeekCollection();
}