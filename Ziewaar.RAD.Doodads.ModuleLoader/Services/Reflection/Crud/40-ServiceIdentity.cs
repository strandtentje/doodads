#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.RKOP.Constructor.Shorthands;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection.Crud;
#pragma warning disable 67
public class ServiceIdentity : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register is not ServiceDescription<ServiceBuilder> description)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected service expression"));
        else if (description.CurrentConstructor is not ISerializableConstructor constructor)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Service requires constructor"));
        else
            OnThen?.Invoke(this, new CommonInteraction(interaction, memory: new SwitchingDictionary(["type", "primary"],
                key => key switch
                {
                    "type" => constructor.ServiceTypeName ?? "",
                    "primary" => constructor.PrimarySettingValue ?? "",
                    _ => throw new KeyNotFoundException(),
                })));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}