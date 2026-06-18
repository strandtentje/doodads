namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;

#pragma warning disable 67
[Category("Parsing & Composing")]
[Title("Trim numbers at start of text")]
[Description("""
             Provided optional number count and number terminator sign, removes numbers
             at the start of a string and sticks them into memory.
             """)]
public class TrimNumberPrefix : IService
{
    [NamedSetting("count", "Amount of numbers we expect")]
    private readonly UpdatingKeyValue CountConst = new("count");
    [NamedSetting("terminator", "Termination character we expect")]
    private readonly UpdatingKeyValue TerminatorConst = new("terminator");
    
    [EventOccasion("Register output here will never have start with numbers according to our pattern")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var incoming = interaction.Register.ToString() ?? "";

        var count = constants.NamedItems.TryGetValue("count", out var ct) ? Convert.ToInt32(ct) : 3;
        var terminator = constants.NamedItems.TryGetValue("terminator", out var tm) &&
                         tm.ToString() is { Length: > 0 } termString
            ? termString
            : ".";

        if (incoming.Take(count).All(char.IsNumber) && incoming.ElementAtOrDefault(count) == terminator[0])
            OnThen?.Invoke(this,
                interaction.AppendRegister(incoming.Substring(count + 1))
                    .AppendMemory(("trimmed", incoming.Substring(0, count + 1))));
        else
            OnThen?.Invoke(this, interaction);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}