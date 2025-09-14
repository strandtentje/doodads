#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.RKOP.Constructor;
using Ziewaar.RAD.Doodads.RKOP.Constructor.Shorthands;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection.Crud;
#pragma warning disable 67
public class ConstantLister : IService
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
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
            return;
        }

        IList<ServiceConstantsMember> members = [];

        if (interaction.Register is RegularNamedConstructor regular)
            members = regular.Constants.Members;
        else if (interaction.Register is CapturedShorthandConstructor captured)
            members = captured.Constants.Members;

        (this, this.CurrentRepeatName, members).RepeatInto(interaction, OnElse, OnThen, member => member,
            member => [("key", member?.Key ?? "")]);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}