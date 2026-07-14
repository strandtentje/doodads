#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.ReadWrite;

[Category("System & IO")]
[Title("Reads ini file into memory")]
[Description("""
             Provided a path in register, and permissible fields in constants with their 
             defaults, will read an ini file.
             """)]
[ShortNames("rini")]
public class ReadIniFile : IService
{
    [EventOccasion("Ini file comes out here as memory")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var registerPath =
            constants.PrimaryConstant.IsntJustAnObject()
                ? constants.PrimaryConstant.ToString()
                : interaction.Register.ToString();
        OnThen?.Invoke(this, interaction.AppendMemory(
            new FallbackReadOnlyDictionary(IniDictionary.FromFile(registerPath),
                constants.NamedItems)));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}