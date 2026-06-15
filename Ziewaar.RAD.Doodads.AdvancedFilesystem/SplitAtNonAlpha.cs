using System.Text;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem;

#pragma warning disable 67
public class SplitAtNonAlpha : IteratingService
{
    protected override bool RunElse { get; }

    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (repeater.Register?.ToString() is not string splitText)
            throw new Exception("string in register req'd");

        splitText = splitText.RemoveDiacritics();

        return splitText.SplitAtNonAlpha().Select(repeater.AppendRegister);
    }
}