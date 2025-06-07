using System;
using System.Collections.Generic;
using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.Lifecycle;

public class Hold : IService, IDisposable
{
    private List<ResidentialInteraction> history = new();
    public event EventHandler<IInteraction> OnError;
    [DefaultBranch]
    public event EventHandler<IInteraction> Continue;
    public event EventHandler<IInteraction> Name;
    public void Dispose()
    {
        foreach (var item in history)
            item.Dispose();
    }
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        string SourceSetting(EventHandler<IInteraction> forwardSourcing, string name, string fallback) =>
            (this, serviceConstants, interaction, forwardSourcing).SourceSetting(name, fallback);
        var name = SourceSetting(this.Name, "name", "default");
        var currentResident = ResidentialInteraction.CreateBlocked(
            interaction, name);
        history.Add(currentResident);
        Continue?.Invoke(this, currentResident);
        currentResident.Enter();
        history.Remove(currentResident);
    }
}
