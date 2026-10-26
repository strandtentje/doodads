namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Lines;

[Category("System & IO")]
[Title("Read line from file, or write default")]
[Description("Reads first line from text file, and if theres none, writes it and returns that in register.")]
[ShortNames("efl")]
public class EnsureFileLine : BasicService
{
    [PrimarySetting("Default line format")]
    private readonly UpdatingPrimaryValue Format = new UpdatingPrimaryValue();
    [EventOccasion("Always has line in register")]
    public override event CallForInteraction? OnThen;
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        var file = Register(interaction);
        string contents;
        if (!File.Exists(file) || 
            File.ReadAllLines(file) is not { } lines || 
            lines.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() is not { } filteredLines || 
            filteredLines.Length < 1)
        {
            var dir = Path.GetDirectoryName(file);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            contents = string.Concat(PrimaryPartsFromMemory(constants, interaction));
            File.WriteAllText(file, contents);
        }
        else
        {
            contents = filteredLines.First();
        }
        OnThen?.Invoke(this, interaction.AppendRegister(contents));
    }
}