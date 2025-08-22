using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem;

public class UnsetFileLink : IService
{
    public event CallForInteraction? LinkParentDir;
    public event CallForInteraction? LinkPath;

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var parentSink = new TextSinkingInteraction(interaction);
        LinkParentDir?.Invoke(this, parentSink);
        var candidateParentPath = parentSink.ReadAllText();

        var linkSink = new TextSinkingInteraction(interaction);
        LinkPath?.Invoke(this, linkSink);
        var candidateLinkName = linkSink.ReadAllText();

        var fullLinkPath = Path.Combine(candidateParentPath, candidateLinkName);

        if (candidateParentPath is not string directoryPath
            || !Directory.Exists(directoryPath)
            || !SymlinkHelper.IsSymlink(fullLinkPath))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction,
                "Existing file link path required for this"));
            return;
        }
        SymlinkRepository.Instance.Delete(fullLinkPath);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
