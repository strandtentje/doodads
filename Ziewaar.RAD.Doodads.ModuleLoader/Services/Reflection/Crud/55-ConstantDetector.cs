#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.RKOP.Constructor;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection.Crud;
#pragma warning disable 67
public class ConstantDetector : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnString, OnTrue, OnFalse, OnNumber, OnArray;
    public event CallForInteraction? OnRelativePath, OnConfigPath, OnProfilePath, OnQueryPath, OnTemplatePath;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (string.IsNullOrWhiteSpace(this.CurrentRepeatName) || this.CurrentRepeatName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
            return;
        }
        if (interaction.Register is not ServiceConstantsMember member)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected service constant"));
            return;
        }
        switch (member?.Value.ConstantType)
        {
            case ConstantType.String:
                OnString?.Invoke(this, new CommonInteraction(
                    interaction, memory: new Dictionary<string, object>()
                    {
                        ["value"] = member.Value.TextValue,
                    }));
                break;
            // ReSharper disable once RedundantBoolCompare
            case ConstantType.Bool when member.Value.BoolValue == false:
                OnFalse?.Invoke(this, interaction);
                break;
            // ReSharper disable once RedundantBoolCompare
            case ConstantType.Bool when member.Value.BoolValue == true:
                OnTrue?.Invoke(this, interaction);
                break;
            case ConstantType.Number:
                OnNumber?.Invoke(this, new CommonInteraction(
                    interaction, memory: new Dictionary<string, object>()
                    {
                        ["value"] = member.Value.NumberValue,
                    }));
                break;
            case ConstantType.Path when member.Value.PathValue.prefix == 'f':
                OnRelativePath?.Invoke(this, new CommonInteraction(
                    interaction, memory: new Dictionary<string, object>()
                    {
                        ["full"] = member.Value.PathValue.ToString(),
                        ["value"] = member.Value.PathValue.RelativePath
                    }));
                break;
            case ConstantType.Path when member.Value.PathValue.prefix == 'c':
                OnConfigPath?.Invoke(this, new CommonInteraction(
                    interaction, memory: new Dictionary<string, object>()
                    {
                        ["full"] = member.Value.PathValue.ToString(),
                        ["value"] = member.Value.PathValue.RelativePath
                    }));
                break;
            case ConstantType.Path when member.Value.PathValue.prefix == 'p':
                OnProfilePath?.Invoke(this, new CommonInteraction(
                    interaction, memory: new Dictionary<string, object>()
                    {
                        ["full"] = member.Value.PathValue.ToString(),
                        ["value"] = member.Value.PathValue.RelativePath
                    }));
                break;
            case ConstantType.Path when member.Value.PathValue.prefix == 't':
                OnTemplatePath?.Invoke(this, new CommonInteraction(
                    interaction, memory: new Dictionary<string, object>()
                    {
                        ["full"] = member.Value.PathValue.ToString(),
                        ["value"] = member.Value.PathValue.RelativePath
                    }));
                break;
            case ConstantType.Path when member.Value.PathValue.prefix == 'q':
                OnQueryPath?.Invoke(this, new CommonInteraction(
                    interaction, memory: new Dictionary<string, object>()
                    {
                        ["full"] = member.Value.PathValue.ToString(),
                        ["value"] = member.Value.PathValue.RelativePath
                    }));
                break;
            case ConstantType.Array:
                var repeater = new RepeatInteraction(this.CurrentRepeatName, interaction);
                repeater.IsRunning = true;
                for (int i = 0; repeater.IsRunning && i < member.Value.ArrayItems.Length; i++)
                {
                    repeater.IsRunning = false;
                    OnThen?.Invoke(this, new CommonInteraction(
                        interaction, register: member.Value.ArrayItems[i]));
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}