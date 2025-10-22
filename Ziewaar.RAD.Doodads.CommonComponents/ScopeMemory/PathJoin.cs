#nullable enable
using System.Collections;
using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.ScopeMemory;

[Category("Memory & Register")]
[Title("Join a path into register")]
[Description("""
             Provide array in primary constant of either path literals or {placeholders} 
             to join up a path.
             """)]
public class PathJoin : IService
{
    [PrimarySetting("Members to join")]
    private readonly UpdatingPrimaryValue JoinMembersConstant = new();
    private string[] CurrentPathMembers = [];
    [EventOccasion("Joined path comes out here.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When there were no path members")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, JoinMembersConstant).IsRereadRequired(out IEnumerable? members))
            this.CurrentPathMembers = members?.OfType<object>().Select(x => x.ToString()).ToArray() ?? [];
        if (CurrentPathMembers.Length == 0)
        {
            OnException?.Invoke(this, interaction.AppendRegister("Path members required"));
            return;
        }
        var formattedPathMembers = CurrentPathMembers.Select(FormatMember(interaction)).ToArray();
        var finalPath = string.Join(Path.DirectorySeparatorChar.ToString(), formattedPathMembers);
        if (Directory.Exists(finalPath))
        {
            var dirInfo = new DirectoryInfo(finalPath);
            OnThen?.Invoke(this, interaction.AppendRegister(dirInfo));
        } else
        {
            var fileInfo = new FileInfo(finalPath);
            OnThen?.Invoke(this, interaction.AppendRegister(fileInfo));
        }
    }

    private Func<string, string> FormatMember(IInteraction interaction)
    {
        return format =>
        {
            var parts = $" {format} ".Split('{', '}').ToArray();
            StringBuilder partBuilder = new();
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (i % 2 == 0)
                {
                    partBuilder.Append(part.Trim());                    
                }
                else if (interaction.TryFindVariable(part.Trim(), out object? candidate) && candidate != null)
                {
                    partBuilder.Append(candidate.ToString());
                }
            }
            return partBuilder.ToString().Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        };
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
