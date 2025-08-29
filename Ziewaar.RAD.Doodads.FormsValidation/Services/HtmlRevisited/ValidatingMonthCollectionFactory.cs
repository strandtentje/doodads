namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class ValidatingMonthCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new MonthValidatingCollection();
}