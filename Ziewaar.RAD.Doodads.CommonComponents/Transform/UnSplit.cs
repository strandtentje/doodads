#pragma warning disable 67
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;

public class Unsplit : BasicService
{
    public override event CallForInteraction? OnThen;
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForCustom<SplitterSetup>(interaction, out var origin);
        BasicException.ForInteraction<RepeatInteraction>(interaction, out var repeatInteraction);
        var position = repeatInteraction.GetCounterValue();
        var arr = origin.Items;
        if (position >= arr.Length)
        {
            Array.Resize(ref arr, position + 1);
            for (int i = 0; i < arr.Length; i++)
                if (arr[i] == null) arr[i] = string.Empty;
        }

        BasicException.ForNullOrEmpty(Register(interaction), "some text required to put in place", out var txt);
        arr[position] = txt;
        var joined = string.Join(origin.Splitter, arr);
        OnThen?.Invoke(this, interaction.AppendRegister(joined));
    }
}
