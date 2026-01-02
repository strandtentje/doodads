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

public class InsertIntoSeries : IService
{
    private readonly UpdatingPrimaryValue PositioningConstant = new();
    private string PositionAt = "before";
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, PositioningConstant).IsRereadRequired(out string? newPosition))
            PositionAt = newPosition ?? "before";
        if (PositionAt is not "before" and not "after")
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "before or after!"));
            return;
        }
        
        var tsi = new TextSinkingInteraction(interaction);
        OnElse?.Invoke(this, tsi);
        var requestedServiceName = tsi.ReadAllText();
        if (!TypeRepository.Instance.TryTestName(requestedServiceName, out string bestMatch))
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, $"Service not found; did you mean {bestMatch}"));
            return;
        }

        if (interaction.Register is not SerializableServiceSeries<ServiceBuilder> series)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, $"May only insert into series."));
            return;
        }

        ServiceExpression<ServiceBuilder> target = series;
        do
        {
            target = series.CreateChild();
            series.Children.Add(target);
        } while (target is not ServiceDescription<ServiceBuilder>);

    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}