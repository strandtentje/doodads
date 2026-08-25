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

[Category("Memory & Register")]
[Title("Naively Join a path into register")]
[Description("""
             Provide array in primary constant of either path literals or {placeholders} 
             to join up a path, and make sure it exists as a directory.
             """)]
[ShortNames("mkdir")]
public class DirCreatingPathJoin : PathJoin
{
    [EventOccasion("Joined dir path comes out here")]
    public override event CallForInteraction? OnThen;
    [EventOccasion("When the file already exists")]
    public override event CallForInteraction? OnException;
    protected override void HandleCombinedPath(IInteraction interaction, string finalPath)
    {
        if (File.Exists(finalPath))
            OnException?.Invoke(this, interaction.AppendRegister("dir creating path join found file with same name to exist"));
        if (!Directory.Exists(finalPath))
            Directory.CreateDirectory(finalPath);
        if (SendToVariable is string targetVar)
            interaction = interaction.AppendMemory((targetVar, finalPath));
        OnThen?.Invoke(this, interaction.AppendRegister(finalPath));
    }
}