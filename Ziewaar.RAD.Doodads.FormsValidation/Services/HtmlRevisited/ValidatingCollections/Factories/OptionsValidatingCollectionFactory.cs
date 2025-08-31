namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class OptionsValidatingCollectionFactory(string[] validOptions) : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new OptionsValidatingCollection(validOptions);
}