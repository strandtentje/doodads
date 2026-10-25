namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;

[Category("Printing & Formatting")]
[Title("Split and iterate path in register")]
[Description("""
             Provide a url-style (forward slash) path in register. It will come out split OnThen, 
             having the entire path in the `path` memory position, 
             and the current part of the path in the `name` memory position.
             """)]
public class Breadcrumbs : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForNullOrEmpty(Register(interaction), "path req'd in register", out var path); 
        RepeatToMemory(constants, interaction, GetBreadcrumbs(path));
    }

    private IEnumerable<IReadOnlyDictionary<string, object>> GetBreadcrumbs(string path)
    {
        for (
            var sc = path.PullString("/", out var element, '/');
            element != "/";
            sc.PullString("/", out element))
            yield return MakeDict(sc, element);
    }

    private static SwitchingDictionary MakeDict(StringCursor sc, string element) =>
        new(["path", "name"], key => key switch
        {
            "path" => sc.AlreadyRead,
            "name" => element,
            _ => throw new KeyNotFoundException(),
        });
}