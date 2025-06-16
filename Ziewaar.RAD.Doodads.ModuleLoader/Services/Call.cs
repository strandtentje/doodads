#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
[Category("Call Definition Return")]
public class Call : IService
{
    private readonly UpdatingPrimaryValue ModuleNameConstant = new();
    private string? CurrentModuleName;
    private string? DefinitionFile;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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
        
        if (string.IsNullOrWhiteSpace(requestedFileComponent) || !File.Exists(requestedFileComponent) && Directory.Exists(requestedFileComponent))
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
            OnException?.Invoke(this, new CommonInteraction(interaction, "may only wildcard file XOR definition, not both"));
            return;
        }
        if (requestedFileComponent == "*")
        {
            if (interaction.Register is not string requestedExternalFile)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "wildcard call defined, but no text in register"));
                return;
            }
            requestedFileComponent = requestedExternalFile;
        } else if (requestedDefComponent == "*")
        {
            if (interaction.Register is not string requestedExternalDef)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "wildcard call defined, but no text in register"));
                return;
            }
            requestedDefComponent = requestedExternalDef;
        }
        
        EnterDefinition(interaction, constants, requestedFileComponent.Trim(), requestedDefComponent.Trim());

    }
    private void EnterDefinition(IInteraction interaction, StampedMap constants, string file, string definition)
    {
        var ci = new CallingInteraction(interaction, constants.NamedItems);
        ci.OnThen += OnThen;
        ci.OnElse += OnElse;
        var program = ProgramRepository.Instance.GetForFile(file);

        program.Emitter.WorkingState.SloppilyWaitForWorkToCease();

        var entryPointCount = program.TryFindEntryPoint(x => x.Name == definition, out var entryPoint);
        if (entryPointCount != 1 || entryPoint == null)
        {
            OnException?.Invoke(
                this,
                new CommonInteraction(
                    interaction,
                    $"entry point should be defined exactly once, but {definition} exists {entryPointCount} times in {file}"));
        }
        else
        {
            entryPoint.Run(this, ci);
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}