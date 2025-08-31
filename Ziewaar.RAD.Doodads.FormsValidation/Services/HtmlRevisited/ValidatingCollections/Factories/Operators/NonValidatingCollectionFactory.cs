namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class NonValidatingCollectionFactory : IValidatingCollectionFactory
{
    public static readonly NonValidatingCollectionFactory Instance = new();
    public IValidatingCollection Create() => new NonValidatingCollection();
}