using Define.Doodads.Expo.Timeline;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;

namespace Ziewaar.RAD.Doodads.Data.FlatFile.Support;

public class KeyValueFile : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register.ToString() is not string kvFileCandidate ||
            string.IsNullOrWhiteSpace(kvFileCandidate))
            throw new BasicException("kv file path required in register");

        JsonFile ourFile = JsonFileRepository.Instance.Retrieve(kvFileCandidate);
    }
}
