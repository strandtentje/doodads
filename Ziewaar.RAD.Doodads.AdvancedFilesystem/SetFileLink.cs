#pragma warning disable 67

using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem;

[Category("System & IO")]
[Title("Set a file link")]
[Description("""
             Provided a parent dir and, link name, a path to link to (in register), make a file symlink
             """)]
public class SetFileLink : IService
{
    [EventOccasion("Sink parent directory here")]
    public event CallForInteraction? LinkParentDir;
    [EventOccasion("Set link name here")]
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

        var candidateTargetPath = interaction.Register?.ToString();

        if (candidateParentPath is not string directoryPath
            || !Directory.Exists(directoryPath)            
            || candidateTargetPath is not string filePath
            || !File.Exists(candidateTargetPath))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction,
                "Check linkparentdir, linkpath, and full file path in memory"));
            return;
        }
        SymlinkRepository.Instance.Update(fullLinkPath, candidateTargetPath, false);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
