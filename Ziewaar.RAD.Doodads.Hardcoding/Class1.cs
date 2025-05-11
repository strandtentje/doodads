using System;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Hardcoding;

public class Service<TService> where TService : IService, new()
{
#nullable enable
    public static EventHandler<IInteraction> Configure(
        ServiceConstants? configuration, 
        params Action<TService>[] chain)
    {
        var instance = new TService();
        foreach (var action in chain)
        {
            action(instance);
        }
        instance.OnError += (s, e) =>
        {
            Console.WriteLine($"for {s.GetType().Name}, {e.Variables["error"]}");
        };
        return (sender, interaction) =>
        {
            instance.Enter(configuration ?? [], interaction);
        };
    }
#nullable disable
}
