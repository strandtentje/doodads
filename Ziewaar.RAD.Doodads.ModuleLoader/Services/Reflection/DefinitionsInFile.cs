namespace Ziewaar.RAD.Doodads.ModuleLoader.Reflection;
[Category("Reflection")]
[Title("Get all the definitions that exist in the file.")]
[Description("""
             Provided with a full file path in Register, will enumerate the definitions that exist in it.
             Enumeration goes into memory at `names`, path to file will be kept in register, and memory, at `path`.
             """)]
public class DefinitionsInFile : IService
{
    [PrimarySetting("Optionally hardcoded rkop path")]
    private readonly UpdatingPrimaryValue ServicePath = new();
    [EventOccasion("with a list of definition names in register, and the path to the file in memory at `path`")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [EventOccasion("likely happens when the path couldn't be determined")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, ServicePath).IsRereadRequired<string>(out var hardcodedServicePath);
        var requestedPath = hardcodedServicePath ?? interaction.Register as string;
        if (interaction.TryFindVariable("path", out string? memoryPath))
            requestedPath = memoryPath;
        if (requestedPath == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "rkop path must be provided via memory, register or configuration."));
            return;
        }
        var definitionNames = ProgramRepository.Instance.GetForFile(requestedPath).Definitions?.Select(x => x.Name)
            ?.ToArray() ?? [];
        OnThen?.Invoke(this, new CommonInteraction(interaction, requestedPath, new SortedList<string, object>()
        {
            { "names", definitionNames },
            { "path", requestedPath }
        }));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}