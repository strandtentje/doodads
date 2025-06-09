namespace Ziewaar.RAD.Doodads.ModuleLoader.Interactions;

public class ServiceDescriptionInteraction(IInteraction parent, string leadingPath, KnownProgram root, ServiceDescription<ServiceBuilder> definition) : IInteraction
{
    public KnownProgram Root => root;
    public ServiceDescription<ServiceBuilder> Definition => definition;
    public IInteraction Parent => parent;
    public IReadOnlyDictionary<string, object> Variables { get; } =
    new SortedList<string, object>(5)
    {
        { "dirname", definition.TextScope.WorkingDirectory.Name },
        { "fulldirname", definition.TextScope.WorkingDirectory.FullName },
        { "filename", definition.TextScope.BareFile },
        { "servicepath", $"{leadingPath}/{definition.ConstantsDescription.BranchKey}" },
        { "branchname", definition.ConstantsDescription.BranchKey }
    };
}
