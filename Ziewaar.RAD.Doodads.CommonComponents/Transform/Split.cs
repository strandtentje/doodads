#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;

[Category("Parsing & Composing")]
[Title("Turn text into a list")]
[Description("""
             Does a best effort to turn Register contents into a string if it isn't already,
             and uses the configured split character to turn the string into a list of strings
             and iterate it.
             """)]
public class Split : BasicService
{
    [PrimarySetting("Splitting character; defaults to forward slash `/`")]
    private readonly UpdatingPrimaryValue SplitCharacter = new();
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForNullOrEmpty(Register(interaction), "string expected in register", out string toSplit);
        var splitter = Named(constants, "splitter") ?? "/";
        var items = toSplit.Split(splitter, (StringSplitOptions)3);
        RepeatToRegister(constants, interaction.AppendCustom(new SplitterSetup(items, splitter)), items);
    }
}
