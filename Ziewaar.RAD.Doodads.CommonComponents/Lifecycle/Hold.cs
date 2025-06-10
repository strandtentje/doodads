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
    public event CallForInteraction? OnThen;
    public event CallForInteraction? Name;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, LockNameConstant).IsRereadRequired(out string? lockName);
        string? desiredName = lockName;
        if (desiredName == null)
        {
            var nameSource = new TextSinkingInteraction(interaction);
            Name?.Invoke(this, interaction);
            using (var rd = nameSource.GetDisposingSinkReader())
            {
                desiredName ??= rd.ReadToEnd();
            }
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
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}