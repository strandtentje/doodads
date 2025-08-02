#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.IO;

[Category("Filesystem")]
[Title("User Directory")]
[Description("""Put path to /home/user or C:\\Users\\User into register""")]
public class UserProfileDirectory : IService
{
    [EventOccasion("When the user profile dir was found and put into register")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) =>
        OnThen?.Invoke(this, new CommonInteraction(
            interaction,
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}