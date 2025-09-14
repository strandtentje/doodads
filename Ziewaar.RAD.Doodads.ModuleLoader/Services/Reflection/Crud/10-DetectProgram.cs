#nullable enable
using Ziewaar.RAD.Doodads.ModuleLoader.Filesystem;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection.Crud;
#pragma warning disable 67
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