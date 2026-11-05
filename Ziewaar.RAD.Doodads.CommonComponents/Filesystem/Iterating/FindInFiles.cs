namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Iterating;

#pragma warning disable 67
[Category("System & IO")]
[Title("Recursively look for files with extension")]
[Description("""
             Provide existing directory in Register, and one or multiple extensions in the primary constant,
             it will look for all files in the provided directory recursively and iterate through the ones with
             matching extensions OnThen.
             """)]
public class FindInFiles : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        var directory = new DirectoryInfo(DirectoryFromRegister(interaction));
        var extensions = PrimaryParts(constants, []);
        var files = directory.EnumerateFiles("*.*", SearchOption.AllDirectories).Where(x => !x.IsHidden()).Where(x =>
            extensions.Length == 0 || extensions.Contains(x.Extension, StringComparer.OrdinalIgnoreCase));
        var payloads = files.Select(x => new FilesystemInfoPayload(x, null, null));
        RepeatToMemory(constants, interaction, payloads);
    }
}

public class FilterFiles : BasicService
{
    private readonly UpdatingKeyValue DistinctByConstant = new UpdatingKeyValue("distinctby");
    public event CallForInteraction? Filter;

    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        var directory = new DirectoryInfo(DirectoryFromRegister(interaction));
        var sink = this.LinesSink(interaction, out var lines);
        Filter?.Invoke(this, sink);
        BasicException.ForNullOrEmpty(lines.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
            "it is required to sink a filter", out var filter);
        var files = directory.EnumerateFiles(filter, SearchOption.AllDirectories);
        var payloads = files.Select(x => new FilesystemInfoPayload(x, null, null));
        switch (Named(constants, DistinctByConstant)?.ToLower())
        {
            case "cleanname":
                payloads = payloads.Distinct(NameComparer.Instance);
                break;
        }

        RepeatToMemory(constants, interaction, payloads);
    }
}

public class NameComparer : IEqualityComparer<FilesystemInfoPayload>
{
    public static readonly NameComparer Instance = new NameComparer();
    public bool Equals(FilesystemInfoPayload x, FilesystemInfoPayload y)=>
        StringComparer.OrdinalIgnoreCase.Equals(x.CleanestPossibleName, y.CleanestPossibleName);
    public int GetHashCode(FilesystemInfoPayload obj) => obj.CleanestPossibleName.GetHashCode();
}