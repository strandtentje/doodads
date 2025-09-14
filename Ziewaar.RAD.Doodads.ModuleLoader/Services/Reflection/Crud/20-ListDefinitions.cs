#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.ModuleLoader.Filesystem;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection.Crud;
#pragma warning disable 67
public class ListDefinitions : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (string.IsNullOrWhiteSpace(this.CurrentRepeatName) || this.CurrentRepeatName == null)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
        else if (interaction.Register is not ProgramFileLoader loader)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected loader from DetectProgram"));
        else
            (this, this.CurrentRepeatName, loader.Definitions ?? []).RepeatInto(interaction, OnElse, OnThen, x => x,
                x => [("name", x.Name)]);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}