using Define.Doodads.Expo.Timeline;
using System;
using System.Text;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Data.FlatFile.Support;

namespace Ziewaar.RAD.Doodads.Data.FlatFile;

[Category("Databases & Querying")]
[Title("Open a Flatfile")]
[Description("""
    Opens a flatfile by the path provided in register, and sticks it into OnThen
    for use with the other flatfile instructions.
    """)]
public class UseFlatFile : BasicService
{
    public override event CallForInteraction? OnThen;
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register.ToString() is not string flatFilePath)
            throw new BasicException("flat file path expected");
        var file = JsonFileRepository.Instance.Retrieve(flatFilePath);
        OnThen?.Invoke(this, interaction.AppendCustom(file));
    }
}
