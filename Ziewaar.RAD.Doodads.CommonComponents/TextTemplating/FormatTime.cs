#nullable enable
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
    [EventOccasion("When the timespan was printed")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var timeReader = new ReadTime();
        var dateFormatter = new Format();
        timeReader.OnThen += (s, e) => dateFormatter.Enter(constants, e);
        timeReader.Enter(new StampedMap(""), interaction);
        OnThen?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
