using System.Collections;

namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;

#pragma warning disable 67
public class IterateArray : IteratingService
{
    protected override bool RunElse { get; } = false;

    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (repeater.Register is not IEnumerable e)
            throw new Exception("Expected enumerable");

        foreach (var eo in e)
            yield return repeater.AppendRegister(eo);
    }
}