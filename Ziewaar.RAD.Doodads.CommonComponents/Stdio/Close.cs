#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
[Category("Sourcing & Sinking")]
[Title("Close the line reader")]
[Description("""
             Use in conjunction with Open, for example when you've seen enough lines, or when you've run out.
             """)]
public class Close : IService
{
    [PrimarySetting("Name also given to Open")]
    private readonly UpdatingPrimaryValue LineReaderNameConstant = new();
    [EventOccasion("When the close was successful")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens because there was no preceding open.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, LineReaderNameConstant).IsRereadRequired(out string? lineReaderName);
        if (lineReaderName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "line reader name required"));
            return;
        }
        List<string> seenNames = new(8);
        if (interaction.TryGetClosest<ReadLinesInteraction>(out var cpi, candidate =>
            {
                seenNames.Add(candidate.Name);
                return candidate.Name == lineReaderName;
            }) && cpi != null)
        {
            cpi.Close();
            OnThen?.Invoke(this, interaction);
        }
        else
            OnException?.Invoke(this, new CommonInteraction(interaction,
                $"no reader under name {lineReaderName}; did you mean [{(string.Join(",", seenNames))}]"));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}