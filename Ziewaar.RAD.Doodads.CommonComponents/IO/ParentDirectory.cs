#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.IO;

[Category("Filesystem")]
[Title("Parent Directory")]
[Description("""Get Parent Directory""")]
public class ParentDirectory : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register.ToString() is string path && new DirectoryInfo(path) is DirectoryInfo dirinfo && dirinfo.Exists)
        {
            OnThen?.Invoke(this, new CommonInteraction(interaction, dirinfo.Parent.FullName));
        } else 
        {
            OnElse?.Invoke(this, interaction);
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
