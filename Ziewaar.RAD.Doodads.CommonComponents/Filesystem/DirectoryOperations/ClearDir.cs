#pragma warning disable 67
using Define.Doodads.Expo.Timeline;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.DirectoryOperations;

[Category("System & IO")]
[Title("Clear directory contents")]
[Description(""""Clear everything in a directory"""")]
public class ClearDir : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register.ToString() is not string dirPath)
            throw new BasicException("directory required in register");
        if (DirectoryOperations.Exists(dirPath))
            DirectoryOperations.Delete(dirPath, true);
        DirectoryOperations.CreateDirectory(dirPath);
    }
}