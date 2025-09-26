#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Lifecycle;
[Category("Scheduling & Flow")]
[Title("Release the Hold above")]
[Description("""
             Should be used in conjunction with a Hold having the same name, and tells it there's no need
             to hold up anymore.  
             """)]
public class Release : IService
{
    [PrimarySetting("Name also used on Hold")]
    private readonly UpdatingPrimaryValue LockNameConstant = new();
    [EventOccasion("Happens after the Hold was release")]
    public event CallForInteraction? OnThen;
    [EventOccasion("Get blocking name dynamically")]
    public event CallForInteraction? GetName;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when the name was not provided, or no Hold with this name could be found")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, LockNameConstant).IsRereadRequired(out string? desiredName);
        if (string.IsNullOrWhiteSpace(desiredName))
        {
            var tsi = new TextSinkingInteraction(interaction);
            GetName?.Invoke(this, tsi);
            desiredName = tsi.ReadAllText();
            if (string.IsNullOrWhiteSpace(desiredName))
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "Hold Lock required name"));
                return;
            }
        }
        if (interaction.TryGetClosest<ResidentialInteraction>(out var candidate, x => x.Name == desiredName) && 
            candidate != null)
        {
            candidate.Leave();
            OnThen?.Invoke(this, interaction);
        } else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Cannot terminate non-existent residence."));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
