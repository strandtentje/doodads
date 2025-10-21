#nullable enable
#pragma warning disable 67
using System.Collections;
using Ziewaar.RAD.Doodads.RKOP.Constructor;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;

public class DiscriminateSetting : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public event CallForInteraction? OnString,
        OnFileInDirectory,
        OnBool,
        OnNumber,
        OnArray;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        switch (interaction.Register)
        {
            case string:
                OnString?.Invoke(this, interaction);
                break;
            case FileInWorkingDirectory:
                OnFileInDirectory?.Invoke(this, interaction);
                break;
            case bool:
                OnBool?.Invoke(this, interaction);
                break;
            case decimal:
                OnNumber?.Invoke(this, interaction);
                break;
            case IEnumerable:
                OnArray?.Invoke(this, interaction);
                break;
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}