#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;

[Category("Call Definition Return")]
[Title("Call to Definition in File")]
[Shorthand("<CONSTANTS>")]
[Description("""
             Configured Programs can be put into separate rkop files with separate Definition blocks.
             These Definition blocks can be given a name as their primary constant between the coconut operators 
             like `Definition("Bake Cookies")` - it is encouraged to make these definition names human readable 
             titles with spaces and such. Same for the filenames. Call can then recall them in a way that is understandable 
             like `Call(f"Oven.rkop @ Bake Cookies")` - the `f` before the quotes will make it look from the directory 
             of the current definition file. the @ means a definition name is coming.
             """)]
public class Call : IService
{
    [PrimarySetting("""
                    Name and Definition name separated by an @
                    At least one of those is required. If only a file is given, the first and only Definition
                    without a name in its primary setting will be invoked.
                    If only a Definition name after an @ is given, the current file will be looked at.
                    """)]
    private readonly UpdatingPrimaryValue ModuleNameConstant = new();

    private string? CurrentModuleName;
    private string? DefinitionFile;

    protected virtual bool Destroyer => false;

    [EventOccasion("When the called configuration returns control using ReturnThen")]
    public event CallForInteraction? OnThen;

    [EventOccasion("When the called configuration returns control using ReturnElse")]
    public event CallForInteraction? OnElse;

    [EventOccasion("Likely when a module file or definition name wasn't given or wasn't found")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ModuleNameConstant).IsRereadRequired(out object? candidateModuleName))
        {
            if (candidateModuleName == null)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "configure a file for a Call"));
                return;
            }

            if (string.IsNullOrWhiteSpace(candidateModuleName?.ToString()))
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "no module name provided"));
                return;
            }

            this.CurrentModuleName = candidateModuleName?.ToString();
        }

        var callComponents = CurrentModuleName?.Split('@') ?? [];
        var requestedFileComponent = callComponents.ElementAtOrDefault(0);
        var requestedDefComponent = callComponents.ElementAtOrDefault(1);

        if (string.IsNullOrWhiteSpace(requestedFileComponent) ||
            !File.Exists(requestedFileComponent) && Directory.Exists(requestedFileComponent))
        {
            if (string.IsNullOrWhiteSpace(requestedDefComponent))
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "cannot call self on main definition"));
                return;
            }

            if (this.DefinitionFile == null)
            {
                var myFile = ProgramRepository.Instance.FindFileOf(this);
                this.DefinitionFile = Path.Combine(myFile.workingDirectory, myFile.fileName);
            }

            requestedFileComponent = this.DefinitionFile;
        }
        else
        {
            requestedDefComponent ??= "";
        }

        if (requestedFileComponent == "*" && requestedDefComponent == "*")
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "may only wildcard file XOR definition, not both"));
            return;
        }

        if (requestedFileComponent == "*")
        {
            if (interaction.Register is not string requestedExternalFile)
            {
                OnException?.Invoke(this,
                    new CommonInteraction(interaction, "wildcard call defined, but no text in register"));
                return;
            }

            requestedFileComponent = requestedExternalFile;
        }
        else if (requestedDefComponent == "*")
        {
            if (interaction.Register is not string requestedExternalDef)
            {
                OnException?.Invoke(this,
                    new CommonInteraction(interaction, "wildcard call defined, but no text in register"));
                return;
            }

            requestedDefComponent = requestedExternalDef;
        }

        EnterDefinition(interaction, constants, requestedFileComponent.Trim(), requestedDefComponent.Trim());
    }

    private void EnterDefinition(IInteraction interaction, StampedMap constants, string file, string definition)
    {
        if (Destroyer)
        {
            ProgramRepository.Instance.DisposeFile(file);
            return;
        }

        var ci = new CallingInteraction(interaction, constants.NamedItems);
        ci.OnThen += OnThen;
        ci.OnElse += OnElse;
        var program = ProgramRepository.Instance.GetForFile(file);

        program.Emitter.WorkingState.SloppilyWaitForWorkToCease();

        var entryPointCount = program.TryFindEntryPoint(x => x.Name == definition, out var entryPoint);
        if (entryPointCount < 1 || entryPoint == null)
        {
            OnException?.Invoke(
                this, new CommonInteraction(interaction, $"Entry point not found: {file} @ {definition}"));
        }
        else if (entryPointCount > 1)
        {
            OnException?.Invoke(
                this, new CommonInteraction(interaction, $"Duplicate entry points found; cannot choose: {file} @ {definition}"));
        }
        else
        {
            entryPoint.Run(this, ci);
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}