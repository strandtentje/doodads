#nullable enable
using System.Collections;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;
#pragma warning disable 67

[Title("Select a file based on its name in the Register, and print its contents to output.")]
[Description("""
             Useful for hosting multiple whitelisted files, based on their name. 
             """)]
public class PrintContentByFilenames : IService
{
    [PrimarySetting("Array of permissible file names (full paths)")]
    private readonly UpdatingPrimaryValue AllowedFileConst = new();
    private string[] FileFilter = [];
    private SortedList<string, (PrintContent printer, StampedMap config)> Printers = new();
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [EventOccasion("In case the file in the register was not whitelisted")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens because there was no file in the whitelist.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, AllowedFileConst).IsRereadRequired<IEnumerable>(out var filter))
        {
            this.FileFilter = filter?.OfType<object>().Select(x => x.ToString()).ToArray() ?? [];
            if (this.FileFilter.Length == 0)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "this wont work until you whitelist at least one file in the primary constant array"));
                return;
            }
            Printers.Clear();
            foreach (var item in FileFilter)
            {
                var newPrinter = new PrintContent();
                newPrinter.OnException += OnException;
                Printers.Add(Path.GetFileName(item), (newPrinter, new StampedMap(item)));               
            }
        }
        var requestedFilename = interaction.Register.ToString();
        if (Printers.TryGetValue(requestedFilename, out var combination))
            combination.printer.Enter(combination.config, interaction);
        else
            OnElse?.Invoke(this, new CommonInteraction(interaction, $"file {requestedFilename} not whitelisted"));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}