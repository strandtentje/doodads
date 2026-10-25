namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Iterating;

#pragma warning disable 67
[Category("System & IO")]
[Title("Recursively look for files with extension")]
[Description("""
             Provide existing directory in Register, and one or multiple extensions in the primary constant,
             it will look for all files in the provided directory recursively and iterate through the ones with
             matching extensions OnThen.
             """)]
public class FindInFiles : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        var directory = new DirectoryInfo(DirectoryFromRegister(interaction));
        var extensions = PrimaryParts(constants, []);
        var files = directory.EnumerateFiles("*.*", SearchOption.AllDirectories).Where(x => !x.IsHidden()).Where(x =>
            extensions.Length == 0 || extensions.Contains(x.Extension, StringComparer.OrdinalIgnoreCase));
        var payloads = files.Select(x => new FilesystemInfoPayload(x, null, null));
        RepeatToMemory(constants, interaction, payloads);
    }
}