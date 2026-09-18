using System.Runtime.InteropServices;

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
            OnException?.Invoke(this,
                interaction.AppendRegister("dir creating path join found file with same name to exist"));
        if (!Directory.Exists(finalPath))
            Directory.CreateDirectory(finalPath);
        if (SendToVariable is string targetVar)
            interaction = interaction.AppendMemory((targetVar, finalPath));
        OnThen?.Invoke(this, interaction.AppendRegister(finalPath));
    }
}

[Category("Memory & Register")]
[Title("Join a path into register, prevent escaping from a working directory")]
[Description("""
             Works like path join, but prevents paths that propagate out of the first argument.
             """)]
[ShortNames("untraverse")]
public class SafePathJoin : PathJoin
{
    [EventOccasion("When the path escapes root")]
    public override event CallForInteraction? OnException;

    protected override string JoinPath(string[] members, IInteraction interaction)
    {
        var firstMember = members[0];
        var joined = Path.GetFullPath(base.JoinPath(members, interaction));
        var comparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (joined.StartsWith(firstMember, comparison)) return joined;
        
        OnException?.Invoke(this, interaction.AppendRegister("path traversal is illegal"));
        return "";
    }

    protected override void HandleCombinedPath(IInteraction interaction, string finalPath)
    {
        if (string.IsNullOrWhiteSpace(finalPath)) return;
        base.HandleCombinedPath(interaction, finalPath);
    }
}