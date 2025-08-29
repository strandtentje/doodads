namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class CountingValidatingCollectionFactory(int lowerValueCountLimit, int upperValueCountLimit) : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new CountingValidatingCollection(lowerValueCountLimit, upperValueCountLimit);
}