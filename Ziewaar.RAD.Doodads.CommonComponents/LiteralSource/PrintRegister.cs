namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

#pragma warning disable 67
[Category("Printing & Formatting")]
[Title("Prints register to output")]
[Description("""
             Does as it says on the tin; serializes whatever is in register and writes it to sink.
             """)]
[ShortNames("preg")]
public class PrintRegister : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForNullOrEmpty(Register(interaction), "nothing useful in register", out var txt);
        BasicException.ForInteraction(interaction, out ISinkingInteraction isi);
        isi.Write(txt);
    }
}