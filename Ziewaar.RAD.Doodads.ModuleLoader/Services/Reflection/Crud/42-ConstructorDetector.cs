#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.RKOP.Constructor;
using Ziewaar.RAD.Doodads.RKOP.Constructor.Shorthands;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection.Crud;
#pragma warning disable 67
public class ConstructorDetector : IService
{
    public event CallForInteraction? OnRegular, OnCaptured, OnPrefixed, OnManipulator;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register is not ServiceDescription<ServiceBuilder> description)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected service expression"));
            return;
        }

        switch (description.CurrentConstructor)
        {
            case RegularNamedConstructor constructor:
                OnRegular?.Invoke(this, new CommonInteraction(interaction, register: constructor,
                    memory: new SwitchingDictionary(
                        ["type", "primary"],
                        key => key switch
                        {
                            "type" => constructor.ServiceTypeName ?? "",
                            "primary" => constructor.PrimarySettingValue ?? "",
                            _ => throw new KeyNotFoundException(),
                        })));
                break;
            case CapturedShorthandConstructor captured:
                OnCaptured?.Invoke(this, new CommonInteraction(interaction, register: captured,
                    memory: new SwitchingDictionary(["type", "primary"],
                        key => key switch
                        {
                            "type" => captured.ServiceTypeName ?? "",
                            "primary" => captured.PrimarySettingValue ?? "",
                            _ => throw new KeyNotFoundException(),
                        })));
                break;
            case PrefixedShorthandConstructor prefixed:
                OnPrefixed?.Invoke(this, new CommonInteraction(interaction, register: prefixed,
                    memory: new SwitchingDictionary(["type", "primary"],
                        key => key switch
                        {
                            "type" => prefixed.ServiceTypeName ?? "",
                            "primary" => prefixed.PrimarySettingValue ?? "",
                            _ => throw new KeyNotFoundException(),
                        })));
                break;
            case ContextValueManipulationConstructor manipulator:
                OnManipulator?.Invoke(this, new CommonInteraction(interaction, register: manipulator,
                    memory: new SwitchingDictionary(["type", "primary"], key => key switch
                    {
                        "type" => manipulator.ServiceTypeName ?? "",
                        "primary" => manipulator.PrimarySettingValue ?? "",
                        _ => throw new KeyNotFoundException(),
                    })));
                break;
            default:
                OnException?.Invoke(this, new CommonInteraction(interaction, "Unknown constructor detected"));
                break;
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}