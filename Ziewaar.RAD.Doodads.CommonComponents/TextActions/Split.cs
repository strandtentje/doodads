#pragma warning disable 67
#nullable enable
namespace Define.Content.AutomationKioskShell.ValidationNodes;
public class Split : IService
{
    private readonly UpdatingPrimaryValue SplitCharacter = new();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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