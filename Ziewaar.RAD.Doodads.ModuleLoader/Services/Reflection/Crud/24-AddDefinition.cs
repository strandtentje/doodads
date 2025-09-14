#nullable enable
using Ziewaar.RAD.Doodads.ModuleLoader.Filesystem;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection.Crud;
#pragma warning disable 67
public class AddDefinition : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register is not ProgramFileLoader loader)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected loader from DetectProgram"));
        else if (interaction.Register is not string newDefinitionName)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected new definition name in register"));
        else if (loader.Definitions?.Any(x => x.Name.Equals(newDefinitionName, StringComparison.OrdinalIgnoreCase)) ==
                 true)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Program with this name already exists"));
        else if (CursorText.Create(loader.Emitter.DirectoryInfo, loader.Emitter.FileInfo.Name + $"-{newDefinitionName}",
                     $@"<<""{newDefinitionName}"">>") is not CursorText temporaryText)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Failed to build temporary cursortext"));
        else if (!ProgramDefinition.TryCreate(ref temporaryText, out var newDefinition))
            OnException?.Invoke(this, new CommonInteraction(interaction, "Failed to build new definition"));
        else if (loader.Definitions == null)
            loader.Definitions = [newDefinition];
        else
            loader.Definitions.Add(newDefinition);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}