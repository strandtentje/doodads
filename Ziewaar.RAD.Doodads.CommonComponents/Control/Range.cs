namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Iterate through some number range")]
[Description("""
             Set number range as repeat name / prim const ie
             0-100
             0-{var}
             """)]
public class Range : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        var rangeString = PrimaryPartsFromMemory(constants, interaction);
        var rangeArray = string.Concat(rangeString).Split(['-'], 3, (StringSplitOptions)3);

        decimal start = 0, step = 1, end = 0;

        if (rangeArray.Length == 0)
            throw new BasicException("range required");
        else if (rangeArray.Length == 1)
            end = Convert.ToDecimal(rangeArray[0]);
        else if (rangeArray.Length == 2)
        {
            start = Convert.ToDecimal(rangeArray[0]);
            end = Convert.ToDecimal(rangeArray[1]);
        } else if (rangeArray.Length == 3)
        {
            start = Convert.ToDecimal(rangeArray[0]);
            step = Convert.ToDecimal(rangeArray[1]);
            end = Convert.ToDecimal(rangeArray[2]);
        }

        RepeatToRegister(constants, interaction, GoThroughRange(start, step, end));
    }

    private IEnumerable<object> GoThroughRange(decimal start, decimal step, decimal end)
    {
        for (decimal i = start; i < end; i += step)
            yield return i;
    }
}