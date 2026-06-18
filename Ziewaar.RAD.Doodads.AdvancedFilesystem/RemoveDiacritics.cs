using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem;

#pragma warning disable 67
public class RemoveDiacritics : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register?.ToString() is { } withDiacritics)
            OnThen?.Invoke(this, interaction.AppendRegister(withDiacritics.RemoveDiacritics()));
        else
            OnException?.Invoke(this, interaction.AppendRegister("string is required for this"));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}