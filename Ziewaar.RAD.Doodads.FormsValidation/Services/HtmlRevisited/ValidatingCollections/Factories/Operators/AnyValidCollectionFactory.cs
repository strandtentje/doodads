namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class AnyValidCollectionFactory(params IValidatingCollectionFactory[] validators) : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new AnyValidatingCollection(
        [..validators.Select(x => x.Create())]);
}