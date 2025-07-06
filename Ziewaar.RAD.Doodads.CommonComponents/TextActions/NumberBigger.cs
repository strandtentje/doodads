#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextActions;

[Category("Validation")]
[Title("Check if number is bigger")]
[Description("""
            Provided a number in memory, and a primary constant lower bound, will tell if its bigger or not.
            """)]
public class NumberBigger : IService
{
    [PrimarySetting("Minimum value for the number in memory")]
    private readonly UpdatingPrimaryValue ValueConstant = new();
    [EventOccasion("When the number in memory was bigger")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the number in memory was not bigger")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when there was not a number in memory")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, ValueConstant).IsRereadRequired(out decimal? lowbound);
        if (lowbound == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "number required"));
            return;
        }
        try
        {
            var number = Convert.ToDecimal(interaction.Register);
            if (number > lowbound)
            {
                OnThen?.Invoke(this, interaction);
            }
            else
            {
                OnElse?.Invoke(this, interaction);
            }
        }
        catch (Exception ex)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, $"not a number in memory {ex}"));
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
