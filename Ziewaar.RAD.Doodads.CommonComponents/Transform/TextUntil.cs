#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;
#pragma warning disable 67
public class TextUntil : IService
{
    private readonly UpdatingPrimaryValue TerminatorConst = new();
    private readonly UpdatingKeyValue RemainderConst = new("remainder");
    private string? CurrentTerminator;
    private string? CurrentRemainderVar;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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