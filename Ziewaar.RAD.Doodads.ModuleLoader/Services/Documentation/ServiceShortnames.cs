#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Documentation;

#pragma warning disable 67
[Category("Reflection & Documentation")]
[Title("Enumerate all shorthands for this service")]
[Description("""
             Some extremely common services may be invoked using a shortname instead
             of a full name. This isn't allowed by default but may be enabled by putting
             `shorthand is accepted;`
             atop an rkop file.
             Alternative headers may be
             `shorthand is rejected;`
             `shorthand is discouraged;`
             `shorthand is encouraged;`
             Note, shorthands and shortnames are different things and the above syntax
             is confusing. That was an accident. Shorthands are contractions like 
             ["Continue"] :? "Load" :! "Store"
             Shortnames are mappings like tpl=>FileTemplate and bdq=>BufferedDataQuery
             """)]
public class ServiceShortnames : IteratingService
{
    public override event CallForInteraction? OnElse;
    protected override bool RunElse => false;

    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (repeater.Register is not string serviceName)
            throw new Exception("Service name required in register");
        var names = DocumentationRepository.Instance.GetTypeShortnames(serviceName);
        if (names.Length != 0)
            return names.Select(repeater.AppendRegister);

        OnElse?.Invoke(this, repeater);
        return [];
    }
}