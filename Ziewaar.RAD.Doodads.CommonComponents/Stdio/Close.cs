#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
public class Close : IService
{
    private readonly UpdatingPrimaryValue LineReaderNameConstant = new();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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