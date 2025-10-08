#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;
#pragma warning disable 67

[Category("Input & Validation")]
[Title("Split text in two")]
[Description("""
             Take the text in the register and split it in two.
             Use primary constant to specify terminator.
             Use remainder constant to specify where remainder of 
             text needs to be put in memory.
             """)]
public class TextUntil : IService
{
    [PrimarySetting("Character or text to look for when splitting")]
    private readonly UpdatingPrimaryValue TerminatorConst = new();
    [NamedSetting("remainder", "Memory name at which to put remaining text after terminator")]
    private readonly UpdatingKeyValue RemainderConst = new("remainder");
    private string? CurrentTerminator;
    private string? CurrentRemainderVar;
    [EventOccasion("Text before terminator comes out in register here; remaining text at remainder memory name")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when no terminator was specified")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, TerminatorConst).IsRereadRequired(out string? terminatorCandidate) &&
            terminatorCandidate is not null)
            this.CurrentTerminator = terminatorCandidate;
        if ((constants, RemainderConst).IsRereadRequired(out string? remainderCandidate) &&
            remainderCandidate is not null)
            this.CurrentRemainderVar = remainderCandidate;
        if (string.IsNullOrEmpty(CurrentTerminator))
        {
            OnException?.Invoke(this, interaction.AppendRegister("terminator chars required"));
            return;
        }

        var toTerminate = interaction.Register.ToString();
        var parts = toTerminate.Split([this.CurrentTerminator], 2, StringSplitOptions.RemoveEmptyEntries);
        if (string.IsNullOrWhiteSpace(CurrentRemainderVar) || CurrentRemainderVar == null || parts.Length < 2)
            OnThen?.Invoke(this, interaction.AppendRegister(parts.ElementAtOrDefault(0) ?? ""));
        else
            OnThen?.Invoke(this, interaction.AppendRegister(parts[0]).AppendMemory((CurrentRemainderVar, parts[1])));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}