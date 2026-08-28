#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

[Category("Parsing & Composing")]
[Title("Print Register as Timespan")]
[Description("""
             Use Template {% syntax %} in the primary settings to format timespan.
             The following tags become available for the datetime that was in register:
             {% day %} {% hour %} {% minute %} {% second %} {% milli %} {% tick %}
             You may also use these variations to get totals:
             {% totalday %} {% totalhour %} {% totalminute %} {% totalsecond %} {% totalmilli %} 
             If there was a number in register, it is assumed that it is in seconds.
             If this is not the case, use ReadTime and Format separately to get more control.
             """)]
public class FormatTime : IService
{
    private readonly ReadTime TimeReader = new ReadTime();
    private readonly Format DateFormatter = new Format();
    private StampedMap LastConstants = new StampedMap("");
    public FormatTime()
    {
        TimeReader.OnThen += (s, e) => DateFormatter.Enter(LastConstants, e);
    }

    [EventOccasion("When the timespan was printed")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        TimeReader.Enter(new StampedMap(""), interaction);
        OnThen?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
