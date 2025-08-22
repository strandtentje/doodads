#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.IO;

[Category("System & IO")]
[Title("Deletes file by register")]
[Description("""
             provided a file in the register, deletes it.
             """)]
public class DeleteFile : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        FileSystemInfo? infoToWorkWith = null;
        if (interaction.Register is FileSystemInfo registerInfo)
        {
            infoToWorkWith = registerInfo;
        }
        else if (interaction.Register is object pathObject &&
            pathObject.ToString() is string path)
        {
            if (File.Exists(path))
                infoToWorkWith = new FileInfo(path);
        }
        if (infoToWorkWith is FileInfo info)
            info.Delete();
        else
            OnException?.Invoke(this, new CommonInteraction(interaction, "provided path wasn't a file"));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
