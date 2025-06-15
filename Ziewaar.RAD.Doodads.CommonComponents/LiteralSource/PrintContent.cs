#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

[Category("Output to Sink")]
[Title("Print a File as Content")]
[Description("""
             Combines Print and PrintFile such that the content type of the file is figured out based
             on its extension, and pushed to the output, along with the file size in bytes.
             Then, the file contents are printed.
             """)]
public class PrintContent : IService
{
    [PrimarySetting("Filename to read from")]
    private readonly UpdatingPrimaryValue ConstantFilename = new();
    [NamedSetting("setlength", """
                               Set this to true, to pass down the length of the file as well. Don't do this when 
                               there's multiple files, or they're not too big.
                               """)]
    private readonly UpdatingKeyValue SetContentLength = new("setlength");
    [EventOccasion("Happens when the file was written to output")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when there was no file or it couldn't be written.")]
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