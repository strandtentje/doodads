#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection.Crud;
#pragma warning disable 67
public class BranchLister : IService
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
        else if (interaction.Register is not ServiceDescription<ServiceBuilder> description)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected service expression"));
        else if (description.Children.Branches is not IList<(string key, ServiceExpression<ServiceBuilder> value)>
                 branches)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected branch list on service"));
        else
            (this, this.CurrentRepeatName, branches).RepeatInto(interaction, OnElse, OnThen, x => x.value,
                x => [("name", x.key)]);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}