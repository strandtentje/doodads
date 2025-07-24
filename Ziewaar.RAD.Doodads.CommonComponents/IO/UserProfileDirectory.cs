#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.IO;

[Category("Filesystem")]
[Title("User Directory")]
[Description("""Put path to /home/user or C:\\Users\\User into register""")]
public class UserProfileDirectory : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) =>
        OnThen?.Invoke(this, new CommonInteraction(
            interaction,
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
