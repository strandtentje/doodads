#pragma warning disable 67
using Define.Doodads.Expo.Timeline;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

[Category("System & IO")]
[Title("Clear directory contents")]
[Description(""""Clear everything in a directory"""")]
public class ClearDir : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register.ToString() is not string dirPath)
            throw new BasicException("directory required in register");
        if (Directory.Exists(dirPath))
            Directory.Delete(dirPath, true);
        Directory.CreateDirectory(dirPath);
    }
}