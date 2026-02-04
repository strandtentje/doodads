#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

[Category("System & IO")]
[Title("Reads ini file into memory")]
[Description("""
    Provided a path in register, and permissible fields in constants with their defaults, will read an ini file.
    """)]
[ShortNames("rini")]
public class ReadIniFile : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var registerPath = 
            constants.PrimaryConstant.IsntJustAnObject() ? 
            constants.PrimaryConstant.ToString() : 
            interaction.Register.ToString();
        OnThen?.Invoke(this, interaction.AppendMemory(new FallbackReadOnlyDictionary(IniDictionary.FromFile(registerPath), constants.NamedItems)));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
