using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using File = System.IO.File;

namespace Ziewaar.RAD.Doodads.Multimedia.Tags;

public class ReadMediaTag : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register?.ToString() is not string fileCandidate ||
            !File.Exists(fileCandidate))
        {
            OnException?.Invoke(this, interaction.AppendRegister("file not found"));
        }
        else
        {
            using var tagFile = TagLib.File.Create(fileCandidate);
            var tag = tagFile.Tag;
            OnThen?.Invoke(this, interaction.AppendMemory(new TagMemory(tag)));
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}