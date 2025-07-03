#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents;

[Category("Dates and Times")]
[Title("Print Register as Date Time")]
[Description("""
             Use Template {% syntax %} in the primary settings to format date and time.
             The following tags become available for the datetime that was in register:
             {% year %} {% month %} {% day %} {% hour %} {% hour12 %} {% ampm %} {% minute %} {% second %} 
             """)]
public class FormatDate : IService
{
    [EventOccasion("When the datetime was printed")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var dateReader = new ReadDate();
        var dateFormatter = new Format();
        dateReader.OnThen += (s, e) => dateFormatter.Enter(constants, e);
        dateReader.Enter(new StampedMap(""), interaction);
        OnThen?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
