#nullable enable
#pragma warning disable 67
using System.Globalization;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

[Category("Dates and Times")]
[Title("Read Register into its Date and Time components")]
[Description("""
             A best effort will be made to derive date time components from register contents. These will then be output 
             such that in memory exist:
             year, month, day, hour, hour12, ampm, minute, second
             """)]
public class ReadDate : IService
{
    [PrimarySetting("Optionally name of memory location to get date from")]
    private readonly UpdatingPrimaryValue MemoryNameOfDateConst = new();
    private string? CurrentMemoryName;

    [EventOccasion("Date components")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, MemoryNameOfDateConst).IsRereadRequired(out string? candidateName) && candidateName != null)
        {
            this.CurrentMemoryName = candidateName;
        }

        bool hasDateTime = false;
        DateTime workingDateTime = default;

        void ResolveDateTime()
        {
            if (hasDateTime)
                return;
            var maybeDate = interaction.Register;

            if (CurrentMemoryName != null &&
                !string.IsNullOrWhiteSpace(CurrentMemoryName) &&
                interaction.TryFindVariable<object>(CurrentMemoryName, out object? namedDate))
            {
                maybeDate = namedDate;
            }

            if (maybeDate is DateTime candidateDateTime)
                workingDateTime = candidateDateTime;
            else
                workingDateTime = Convert.ToDateTime(maybeDate, CultureInfo.InvariantCulture);
            hasDateTime = true;
        }

        var resultMemory = new SwitchingDictionary(["year", "month", "day", "hour", "minute", "second", "ampm", "hour12"], keyName =>
        {
            switch (keyName)
            {
                case "year":
                    ResolveDateTime();
                    return workingDateTime.Year;
                case "month":
                    ResolveDateTime();
                    return workingDateTime.Month;
                case "day":
                    ResolveDateTime();
                    return workingDateTime.Day;
                case "hour":
                    ResolveDateTime();
                    return workingDateTime.Hour;
                case "minute":
                    ResolveDateTime();
                    return workingDateTime.Minute;
                case "second":
                    ResolveDateTime();
                    return workingDateTime.Second;
                case "ampm":
                    ResolveDateTime();
                    return workingDateTime.ToString("tt");
                case "hour12":
                    return workingDateTime.ToString("hh");
                default:
                    throw new KeyNotFoundException();
            }
        });

        OnThen?.Invoke(this, new CommonInteraction(interaction, memory: resultMemory));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
