#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents;

[Category("Dates and Times")]
[Title("Read Register into its Date and Time components")]
[Description("""
             A best effort will be made to derive timespan components from register contents. These will then be output 
             such that in memory exist:
             day, hour, minute, second, milli, tick
             You may also use these variations to get totals:
             totalday, totalhour, totalminute, totalsecond, totalmilli
             In case a number is provided, use Significance to configure what unit the number is in. options are:
             day, hour, minute, second, milli, tick
             """)]
public class ReadTime : IService
{
    [PrimarySetting("Optionally name of memory location to get date from")]
    private readonly UpdatingPrimaryValue MemoryNameOfTimeConst = new();
    [NamedSetting("significance", "In case of numeric input, the unit of the number (day, hour, minute, second, milli, tick)")]
    private readonly UpdatingKeyValue SignificanceConst = new("significance");
    [NamedSetting("default", "In case of no timespan in memory, the amount of units to default to")]
    private readonly UpdatingKeyValue DefaultConst = new("default");
    private string? CurrentMemoryName;
    private TimespanSignificance CurrentNumericSignificance = TimespanSignificance.Second;
    private decimal DefaultUnity = 60M;

    [EventOccasion("Date components")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public enum TimespanSignificance { Day, Hour, Minute, Second, Milli, Tick }

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, MemoryNameOfTimeConst).IsRereadRequired(out string? candidateName) && candidateName != null)
            this.CurrentMemoryName = candidateName;
        if ((constants, DefaultConst).IsRereadRequired(out decimal? defaultTimeUnitCandidate) && defaultTimeUnitCandidate is decimal defaultInUnits)
            this.DefaultUnity = defaultInUnits;
        if ((constants, SignificanceConst).IsRereadRequired(out string? significance) &&
            significance != null &&
            Enum.TryParse<TimespanSignificance>(significance, ignoreCase: true, out var candidateSignificance))
        {
            this.CurrentNumericSignificance = candidateSignificance;
        }

        bool hasTimespan = false;
        TimeSpan workingTimeSpan = default;

        void ResolveTimespan()
        {
            if (hasTimespan)
                return;
            var maybeTimespan = interaction.Register;

            if (CurrentMemoryName != null &&
                !string.IsNullOrWhiteSpace(CurrentMemoryName) &&
                interaction.TryFindVariable<object>(CurrentMemoryName, out object? namedTimespan))
            {
                maybeTimespan = namedTimespan;
            }

            if (maybeTimespan is TimeSpan candidateTimespan)
                workingTimeSpan = candidateTimespan;
            else if (maybeTimespan is string candidateString && TimeSpan.TryParse(candidateString, out var candidateFromString))
                workingTimeSpan = candidateFromString;
            else
            {
                decimal numberTime = this.DefaultUnity;
                if (maybeTimespan is not null && maybeTimespan.ConvertNumericToDecimal() is decimal candidateNumberTime)
                    numberTime = candidateNumberTime;
                switch (CurrentNumericSignificance)
                {
                    case TimespanSignificance.Day:
                        workingTimeSpan = TimeSpan.FromDays((double)numberTime);
                        break;
                    case TimespanSignificance.Hour:
                        workingTimeSpan = TimeSpan.FromHours((double)numberTime);
                        break;
                    case TimespanSignificance.Minute:
                        workingTimeSpan = TimeSpan.FromMinutes((double)numberTime);
                        break;
                    case TimespanSignificance.Second:
                        workingTimeSpan = TimeSpan.FromSeconds((double)numberTime);
                        break;
                    case TimespanSignificance.Milli:
                        workingTimeSpan = TimeSpan.FromMilliseconds((double)numberTime);
                        break;
                    case TimespanSignificance.Tick:
                        workingTimeSpan = TimeSpan.FromTicks((long)numberTime);
                        break;
                    default:
                        break;
                }
            }

            hasTimespan = true;
        }

        var validNames = Enum.GetNames(typeof(TimespanSignificance)).Select(x => x.ToLower());
        var allNames = validNames.Concat(validNames.Select(x => $"total{x}")).ToArray();

        var resultMemory = new SwitchingDictionary(allNames, keyName =>
        {
            bool isTotal = false;
            if (keyName.StartsWith("total"))
            {
                isTotal = true;
                keyName = keyName.Substring("total".Length);
            }
            if (!Enum.TryParse(keyName, ignoreCase: true, out TimespanSignificance desiredDigit))
                throw new KeyNotFoundException();
            ResolveTimespan();
            switch (desiredDigit)
            {
                case TimespanSignificance.Day:
                    return (decimal)(isTotal ? workingTimeSpan.TotalDays : workingTimeSpan.Days);
                case TimespanSignificance.Hour:
                    return (decimal)(isTotal ? workingTimeSpan.TotalHours : workingTimeSpan.Hours);
                case TimespanSignificance.Minute:
                    return (decimal)(isTotal ? workingTimeSpan.TotalMinutes : workingTimeSpan.Minutes);
                case TimespanSignificance.Second:
                    return (decimal)(isTotal ? workingTimeSpan.TotalSeconds : workingTimeSpan.Seconds);
                case TimespanSignificance.Milli:
                    return (decimal)(isTotal ? workingTimeSpan.TotalMilliseconds : workingTimeSpan.Milliseconds);
                case TimespanSignificance.Tick:
                    return (decimal)(isTotal ? workingTimeSpan.Ticks : workingTimeSpan.Ticks);
                default:
                    throw new KeyNotFoundException();
            }
        });

        OnThen?.Invoke(this, new CommonInteraction(interaction, memory: resultMemory));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
