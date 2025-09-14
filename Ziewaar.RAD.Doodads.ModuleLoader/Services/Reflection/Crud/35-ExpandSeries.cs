#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection.Crud;
#pragma warning disable 67
public class ExpandSeries : IService
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
        else if (interaction.Register is not SerializableServiceSeries<ServiceBuilder> series)
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "Expected series from ie. DetectServiceOrSeries"));
        else if (series.Children is not IEnumerable<ServiceExpression<ServiceBuilder>> enumerable)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Series requires children."));
        else
            (this, this.CurrentRepeatName, enumerable).RepeatInto(interaction, OnElse, OnThen, member => member);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}