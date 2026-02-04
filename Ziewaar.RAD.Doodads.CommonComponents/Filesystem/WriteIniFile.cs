#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

[Category("System & IO")]
[Title("Reads ini file into memory")]
[Description("""
    Provided a path in register, 
    and permissible fields in constants with their defaults, will write an ini file from memory
    """)]
[ShortNames("wini")]
public class WriteIniFile : IService
{
    public readonly UpdatingPrimaryValue NamesToWrite = new();

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var registerPath =
            constants.PrimaryConstant.IsntJustAnObject() ?
            constants.PrimaryConstant.ToString() :
            interaction.Register.ToString();
        using var writer = new StreamWriter(File.OpenWrite(registerPath));
        foreach (var item in constants.NamedItems)
        {
            if (interaction.TryFindVariable<object>(item.Key, out var foundItem) && foundItem != null)
                writer.WriteLine($"{item.Key}={foundItem}");
            else
                writer.WriteLine($"{item.Key}={item.Value}");
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
