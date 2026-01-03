namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Populate a zip name with another item")]
[Description("""
             This happens in the OnElse of a ZipIn, after ZipEnter.
             Interactions propagated into this services, are saved for later use by ZipOut
             """)]
public class ZipOut : IService
{
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When the zip name wasn't correct")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ZipFocusInteraction>(out var zipInteraction) ||
            zipInteraction == null)
        {
            OnException?.Invoke(this, interaction.AppendRegister("no suitable zip in found"));
            return;
        }
        zipInteraction.BlockingCollection.Add(interaction); 
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}