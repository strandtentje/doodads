using System;
using System.Collections.Generic;
using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.Lifecycle;
#nullable enable
public class Hold : IService, IDisposable
{
    private List<ResidentialInteraction> History = new();

    public void Dispose()
    {
        foreach (var item in History)
            item.Dispose();
    }
    private readonly UpdatingPrimaryValue LockNameConstant = new();
    public event EventHandler<IInteraction>? OnThen;
    public event EventHandler<IInteraction>? Name;
    public event EventHandler<IInteraction>? OnElse;
    public event EventHandler<IInteraction>? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, LockNameConstant).IsRereadRequired(out string? lockName);
        var nameSource = new TextSinkingInteraction(interaction);
        Name?.Invoke(this, interaction);
        string? desiredName = lockName;
        using (var rd = nameSource.GetDisposingSinkReader())
        {
            desiredName ??= rd.ReadToEnd();
        }
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
}