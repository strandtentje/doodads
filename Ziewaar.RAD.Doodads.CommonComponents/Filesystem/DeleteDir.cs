#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

[Category("System & IO")]
[Title("Recursive delete directory")]
[Description("""
    Provided a dir in register, deletes it.
    """)]
[ShortNames("rmrf")]
public class DeleteDir : IService
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
            if (Directory.Exists(path))
                infoToWorkWith = new DirectoryInfo(path);
        }
        if (infoToWorkWith is DirectoryInfo info)
            info.Delete(true);
        else
            OnException?.Invoke(this, new CommonInteraction(interaction, "provided path wasn't a directory"));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}