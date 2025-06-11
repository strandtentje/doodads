#pragma warning disable 67
#nullable enable
using System.Collections;

namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

public class PrintContent : IService
{
    private readonly UpdatingPrimaryValue ConstantFilename = new();
    private readonly UpdatingKeyValue SetContentLength = new("setlength");

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    private readonly Print ContentService = new Print();
    private readonly PrintFile FileSerivce = new PrintFile();
    private StampedMap? ContentTypeSettings;
    private StampedMap? FileServingSettings;

    public PrintContent()
    {
        ContentService.OnException += OnException;
        FileSerivce.OnException += OnException;
    }

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var refreshFile = (constants, ConstantFilename).IsRereadRequired(out object? file);
        var refreshCl = (constants, SetContentLength).IsRereadRequired<bool>(() => true, out var setContentLength);

        if (refreshCl || refreshFile || this.ContentTypeSettings == null || this.FileServingSettings == null)
        {
            if (file == null)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "file is required as primary constant"));
                return;
            }
            var fileInfo = new FileInfo(file.ToString());
            var mime = MimeMapping.GetMimeInfo(fileInfo);
            this.ContentTypeSettings = new("", new SortedList<string, object>()
            {
                { "contenttype", mime.MimeType },
            });
            this.FileServingSettings = new(file, new SortedList<string, object>()
            {
                { "binary", !mime.IsText },
                { "setlength", setContentLength },
            });
        }

        ContentService.Enter(ContentTypeSettings, interaction);
        FileSerivce.Enter(FileServingSettings, interaction);
        OnThen?.Invoke(this, interaction);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class PrintContentByFilenames : IService
{
    private readonly UpdatingPrimaryValue AllowedFileConst = new();
    private string[] FileFilter = [];
    private SortedList<string, (PrintContent printer, StampedMap config)> Printers = new();

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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
                Printers.Add(item, (newPrinter, new StampedMap(item)));               
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
