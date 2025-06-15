#pragma warning disable 67
#nullable enable
namespace Define.Content.AutomationKioskShell.ValidationNodes;
[Category("Text in Register")]
[Title("Turn text into a list")]
[Description("""
             Does a best effort to turn Register contents into a string if it isn't already,
             and uses the configured split character to turn the string into a list of strings.
             The default split character is '/', which makes this suitable for routing duties in 
             conjunction with Pop.
             """)]
public class Split : IService
{
    [PrimarySetting("Splitting character; defaults to forward slash `/`")]
    private readonly UpdatingPrimaryValue SplitCharacter = new();
    [EventOccasion("When a string was indeed turned into a list, puts the list in Register")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when the register content couldn't be turned into a string.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, SplitCharacter).IsRereadRequired(out string? splitChar);
        splitChar ??= "/";
        if (interaction.Register is not string toSplit)
        {
            try
            {
                toSplit = Convert.ToString(interaction.Register);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, ex.ToString()));
                return;
            }
        }

        OnThen?.Invoke(this, new CommonInteraction(
            interaction, toSplit.Split(splitChar, StringSplitOptions.RemoveEmptyEntries)));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}