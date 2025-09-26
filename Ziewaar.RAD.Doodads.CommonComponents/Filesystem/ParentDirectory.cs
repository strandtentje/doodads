#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;
[Category("System & IO")]
[Title("Parent Directory")]
[Description("""Get Parent Directory""")]
public class ParentDirectory : IService
{
    [EventOccasion("When the parent directory exists and is in register")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When no parent directory could be determined")]
    public event CallForInteraction? OnElse;
    [NeverHappens] public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register.ToString() is string path && new DirectoryInfo(path) is DirectoryInfo dirinfo &&
            dirinfo is { Exists: true, Parent: not null })
            OnThen?.Invoke(this, new CommonInteraction(interaction, dirinfo.Parent.FullName));
        else
            OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}