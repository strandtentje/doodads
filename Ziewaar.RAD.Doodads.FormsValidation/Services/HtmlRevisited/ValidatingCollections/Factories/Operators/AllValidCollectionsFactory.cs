namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class AllValidCollectionsFactory(params IValidatingCollectionFactory[] precedents) 
    : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new AndValidatingCollection(
        [..precedents.Select(x => x.Create())]);
}