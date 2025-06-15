using System;
using System.Collections.Generic;
using System.Text;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.CommonComponents.Lifecycle;
#nullable enable
[Title("Blocks from here on out to prevent premature finishing of the execution")]
[Description("""
             Will pass on the interaction simply into OnThen, but will only return control when
             a Release was hit with the same name in the underlying block. This is useful for 
             keeping the application alive, or preventing requests from terminating prematurely,
             but should be using sparingly because it can cause application deadlocks.
             """)]
public class Hold : IService, IDisposable
{
    private List<ResidentialInteraction> History = new();
    public void Dispose()
    {
        foreach (var item in History)
            item.Dispose();
    }
    [PrimarySetting("Name that the Release should also use.")]
    private readonly UpdatingPrimaryValue LockNameConstant = new();
    [EventOccasion("Happens before blocking")]
    public event CallForInteraction? OnThen;
    [EventOccasion("Happens after underlying release was triggered")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when a name was not provided.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, LockNameConstant).IsRereadRequired(out string? lockName);
        string? desiredName = lockName;
        if (string.IsNullOrWhiteSpace(desiredName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Hold Lock required name"));
            return;
        }
        var currentResident = ResidentialInteraction.CreateBlocked(
            interaction, desiredName);
        History.Add(currentResident);
        OnThen?.Invoke(this, currentResident);
        currentResident.Enter();
        History.Remove(currentResident);
        OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}