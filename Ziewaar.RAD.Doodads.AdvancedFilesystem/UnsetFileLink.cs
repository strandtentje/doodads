#pragma warning disable 67

using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem;

[Category("System & IO")]
[Title("Set a file link")]
[Description("""
             Provided a parent dir and, link name, a path to link to (in register), unmake a file symlink
             """)]
public class UnsetFileLink : IService
{
    [EventOccasion("Parent dir of symlink")]
    public event CallForInteraction? LinkParentDir;
    [EventOccasion("Name of symlink")]
    public event CallForInteraction? LinkPath;
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
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
