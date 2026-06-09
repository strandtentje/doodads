#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.ScopeMemory;

[Category("Memory & Register")]
[Title("Naively Join a path into register")]
[Description("""
             Provide array in primary constant of either path literals or {placeholders} 
             to join up a path, but dont check if that path exists
             """)]
[ShortNames("ipj")]
public class IncompletePathJoin : PathJoin
{
    [EventOccasion("Joined path comes out here")]
    public override event CallForInteraction? OnThen;
    protected override void HandleCombinedPath(IInteraction interaction, string finalPath)
    {
        if (SendToVariable is string targetVar)
            interaction = interaction.AppendMemory((targetVar, finalPath));
        OnThen?.Invoke(this, interaction.AppendRegister(finalPath));
    }
}