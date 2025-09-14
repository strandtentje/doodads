using System.Collections;
using System.Collections.ObjectModel;
using System.Data.Common;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.ModuleLoader.Bridge;
using Ziewaar.RAD.Doodads.ModuleLoader.Filesystem;
using Ziewaar.RAD.Doodads.RKOP.Constructor;
using Ziewaar.RAD.Doodads.RKOP.Constructor.Shorthands;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
#pragma warning disable 67
#nullable enable
[Category("Reflection & Documentation")]
[Title("List all directories that contain running programs")]
[Description("""
             For the currently running doodads instance, this service enumerates 
             all open working directories / common ancestors.
             """)]
public class ProgramRootDirectories : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;
    [EventOccasion("A list with 0 or more directory info's")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [NeverHappens] public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (string.IsNullOrWhiteSpace(this.CurrentRepeatName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
            return;
        }

        var allDirs = ProgramRepository.Instance.GetKnownPrograms()
            .Select(x => (x.Emitter.DirectoryInfo!.FullName, x.Emitter.DirectoryInfo));
        var sortedDistinctDirs =
            allDirs.Distinct(new FullNameComparer()).OrderBy(x => x.FullName);

        List<(string FullName, DirectoryInfo DirectoryInfo)> rootDirectories = new();

        var repeater = new RepeatInteraction(this.CurrentRepeatName, interaction);
        string? lastDirectory = null;

        using (var dirEnumerator = sortedDistinctDirs.GetEnumerator())
        {
            repeater.IsRunning = true;
            while (repeater.IsRunning & dirEnumerator.MoveNext())
            {
                if (lastDirectory == null ||
                    dirEnumerator.Current.FullName.StartsWith(lastDirectory, StringComparison.OrdinalIgnoreCase))
                    continue;
                repeater.IsRunning = false;
                OnThen?.Invoke(this, new CommonInteraction(repeater, dirEnumerator.Current.DirectoryInfo));
                lastDirectory = dirEnumerator.Current.DirectoryInfo.FullName;
            }
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
public class DetectProgram : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var candidateProgramPath = interaction.Register.ToString();
        if (!File.Exists(candidateProgramPath))
        {
            OnElse?.Invoke(this, interaction);
            return;
        }

        var candidateProgramFile = new FileInfo(candidateProgramPath);

        var detectedProgramFile = ProgramRepository.Instance.GetKnownPrograms().SingleOrDefault(x =>
            x.Emitter.FileInfo.FullName.Equals(candidateProgramFile.FullName, StringComparison.OrdinalIgnoreCase));

        if (detectedProgramFile is not ProgramFileLoader loader)
        {
            OnElse?.Invoke(this, interaction);
            return;
        }

        OnThen?.Invoke(this, new CommonInteraction(interaction, loader));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
public class AddProgram : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var tsi = new TextSinkingInteraction(interaction);
        OnThen?.Invoke(this, tsi);
        var fileInfo = new FileInfo(tsi.ReadAllText());
        if (!fileInfo.Exists)
        {
            try
            {
                using (var x = fileInfo.CreateText())
                {
                    x.WriteLine();
                }
                OnThen?.Invoke(this, new CommonInteraction(interaction, fileInfo));
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, ex));
            }
        }
        else
        {
            OnElse?.Invoke(this, new CommonInteraction(interaction, fileInfo));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
public class ListDefinitions : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (string.IsNullOrWhiteSpace(this.CurrentRepeatName) || this.CurrentRepeatName == null)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
        else if (interaction.Register is not ProgramFileLoader loader)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected loader from DetectProgram"));
        else
            (this, this.CurrentRepeatName, loader.Definitions ?? []).RepeatInto(interaction, OnElse, OnThen, x => x,
                x => [("name", x.Name)]);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
public class AddDefinition : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register is not ProgramFileLoader loader)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected loader from DetectProgram"));
        else if (interaction.Register is not string newDefinitionName)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected new definition name in register"));
        else if (loader.Definitions?.Any(x => x.Name.Equals(newDefinitionName, StringComparison.OrdinalIgnoreCase)) ==
                 true)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Program with this name already exists"));
        else if (CursorText.Create(loader.Emitter.DirectoryInfo, loader.Emitter.FileInfo.Name + $"-{newDefinitionName}",
                     $@"<<""{newDefinitionName}"">>") is not CursorText temporaryText)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Failed to build temporary cursortext"));
        else if (!ProgramDefinition.TryCreate(ref temporaryText, out var newDefinition))
            OnException?.Invoke(this, new CommonInteraction(interaction, "Failed to build new definition"));
        else if (loader.Definitions == null)
            loader.Definitions = [newDefinition];
        else
            loader.Definitions.Add(newDefinition);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
public class DefinitionSeries : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register is not ProgramDefinition definition)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected definition from ListDefinitions"));
            return;
        }

        OnThen?.Invoke(this, new CommonInteraction(interaction, definition.CurrentSeries));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
public class DetectServiceOrSeries : IService
{
    public event CallForInteraction? OnAlso;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnService;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        switch (interaction.Register)
        {
            case UnconditionalSerializableServiceSeries<ServiceBuilder> ampersandExpression:
                OnAlso?.Invoke(this, new CommonInteraction(interaction, ampersandExpression));
                break;
            case AlternativeSerializableServiceSeries<ServiceBuilder> pipeExpression:
                OnElse?.Invoke(this, new CommonInteraction(interaction, pipeExpression));
                break;
            case ConditionalSerializableServiceSeries<ServiceBuilder> colonExpression:
                OnThen?.Invoke(this, new CommonInteraction(interaction, colonExpression));
                break;
            case ServiceDescription<ServiceBuilder> serviceExpression:
                OnService?.Invoke(this, new CommonInteraction(interaction, serviceExpression));
                break;
            default:
                OnException?.Invoke(this,
                    new CommonInteraction(interaction, "Expected service expression from ie DefinitionSeries"));
                break;
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
public class ExpandSeries : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (string.IsNullOrWhiteSpace(this.CurrentRepeatName) || this.CurrentRepeatName == null)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
        else if (interaction.Register is not SerializableServiceSeries<ServiceBuilder> series)
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "Expected series from ie. DetectServiceOrSeries"));
        else if (series.Children is not IEnumerable<ServiceExpression<ServiceBuilder>> enumerable)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Series requires children."));
        else
            (this, this.CurrentRepeatName, enumerable).RepeatInto(interaction, OnElse, OnThen, member => member);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
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
public class BranchLister : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (string.IsNullOrWhiteSpace(this.CurrentRepeatName) || this.CurrentRepeatName == null)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
        else if (interaction.Register is not ServiceDescription<ServiceBuilder> description)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected service expression"));
        else if (description.Children.Branches is not IList<(string key, ServiceExpression<ServiceBuilder> value)>
                 branches)
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected branch list on service"));
        else
            (this, this.CurrentRepeatName, branches).RepeatInto(interaction, OnElse, OnThen, x => x.value,
                x => [("name", x.key)]);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
public class ConstantLister : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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

        IList<ServiceConstantsMember> members = [];

        if (interaction.Register is RegularNamedConstructor regular)
            members = regular.Constants.Members;
        else if (interaction.Register is CapturedShorthandConstructor captured)
            members = captured.Constants.Members;

        (this, this.CurrentRepeatName, members).RepeatInto(interaction, OnElse, OnThen, member => member,
            member => [("key", member?.Key ?? "")]);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
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