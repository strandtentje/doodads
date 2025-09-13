using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.ModuleLoader.Bridge;
using Ziewaar.RAD.Doodads.ModuleLoader.Filesystem;

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
        if (string.IsNullOrWhiteSpace(this.CurrentRepeatName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
            return;
        }

        if (interaction.Register is not ProgramFileLoader loader)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected loader from DetectProgram"));
            return;
        }

        using (var definitionEnumerator = loader.Definitions.GetEnumerator())
        {
            var repeater = new RepeatInteraction(this.CurrentRepeatName, interaction);
            repeater.IsRunning = true;
            while (repeater.IsRunning && definitionEnumerator.MoveNext())
            {
                repeater.IsRunning = false;
                OnThen?.Invoke(this, new CommonInteraction(repeater, register: definitionEnumerator.Current,
                    memory: new SwitchingDictionary(["definitionname"], key => key switch
                    {
                        "definitionname" => definitionEnumerator.Current.Name,
                        _ => throw new KeyNotFoundException(),
                    })));
            }
        }
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
        if (string.IsNullOrWhiteSpace(this.CurrentRepeatName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
            return;
        }
        
        if (interaction is not SerializableServiceSeries<ServiceBuilder> series)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "Expected series from ie. DetectServiceOrSeries"));
            return;
        }

        using (var seriesMemberEnumerator = series.Children.GetEnumerator())
        {
            var repeater = new RepeatInteraction(this.CurrentRepeatName, interaction);
            repeater.IsRunning = true;
            while (repeater.IsRunning && seriesMemberEnumerator.MoveNext())
            {
                repeater.IsRunning = false;
                OnThen?.Invoke(this, new CommonInteraction(repeater, seriesMemberEnumerator.Current));
            }
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class ExpandService : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction is not ServiceDescription<ServiceBuilder> description)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected service expression"));
            return;
        }
        
        description.
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}