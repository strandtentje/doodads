#nullable enable
#pragma warning disable 67
using System.Collections;
using Ziewaar.RAD.Doodads.RKOP.Constructor;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;

[Category("Reflection & Documentation")]
[Title("Discriminate the reflected setting in regster")]
[Description("Figure out what kind of setting we're looking at in the Register")]
public class DiscriminateSetting : IService
{
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    [EventOccasion("In case we're looking at a string setting")]
    public event CallForInteraction? OnString;
    [EventOccasion("In case we're looking a string thats a path relative to some directory")]
    public event CallForInteraction? OnFileInDirectory;
    [EventOccasion("In case we're looking at a boolean setting")]
    public event CallForInteraction? OnBool;
    [EventOccasion("In case we're looking at a numeric (decimal never float) setting")]
    public event CallForInteraction? OnNumber;
    [EventOccasion("In case we need to go deeper.")]
    public event CallForInteraction? OnArray;

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