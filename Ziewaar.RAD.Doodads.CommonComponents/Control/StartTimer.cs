#nullable enable

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;
public class StartTimer() : TimerCommandSender(TimerCommand.Start);


public class Repeat : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, RepeatNameConstant).IsRereadRequired(out string? repeatName);
        if (repeatName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "repeat name required"));
            return;
        }
        var ri = new RepeatInteraction(repeatName, interaction);
        OnElse?.Invoke(this, ri);
        while(ri.IsRunning)
        {
            ri.IsRunning = false;
            OnThen?.Invoke(this, ri);
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class Continue : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    public event CallForInteraction OnThen;
    public event CallForInteraction OnElse;
    public event CallForInteraction OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, RepeatNameConstant).IsRereadRequired(out string? repeatName);
        if (repeatName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "repeat name required"));
            return;
        }
        List<string> spottedRepeats = new(8);
        if (!interaction.TryGetClosest<RepeatInteraction>(out var ri, x => {
            spottedRepeats.Add(x.RepeatName);
            return x.RepeatName == repeatName;
        }) || 
            ri == null)
        {
            OnException?.Invoke(this, new CommonInteraction(
                interaction, $"could not find repeater with name {repeatName}; did you mean [{(string.Join(",", spottedRepeats))}]"));
            return;
        }
        ri.IsRunning = true;
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class RepeatInteraction(string repeatName, IInteraction parent) : IInteraction
{
    public string RepeatName => repeatName;
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public bool IsRunning = true;
}
