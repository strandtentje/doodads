#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
[Category("Memory & Register")]
[Title("Load value from memory into register")]
[Shorthand("?PRIMARY")]
[Description("""
             Services communicate via interactions. Interactions always have 
              - Memory 
              - Register
              
             Memory is addressed using names, the Register is not; the Register generally
             contains the working value a service may presume it needs to do work on. 
             
             When invoking child services, a service may override things in memory or the
             register, but when control leaves the service to its parent service, those 
             overrides will not persist; hence Memory and Register is scoped as one traverses
             deeper into the program.
             
             This idea puts variables and variable naming into the back seat for focussing on the 
             order of operations instead. However, it does mean, sometimes you need to move something
             into or out of the Register, or Memory. This is what Load and Store are for.
             """)]
[ShortNames("ld")]
public class Load : IService
{
    [PrimarySetting("The memory key to look at for retrieving the Register value")]
    private readonly UpdatingPrimaryValue KeyConstant = new();
    [NamedSetting("default", "When no value was found, store this value instead.")]
    private readonly UpdatingKeyValue DefaultValueConstant = new("default");

    private string? KeyName;
    private object? DefaultValue;
    [EventOccasion("When the value was found in memory, the memory value will be in register from here")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the value was not found in memory, the default value will be in register from here")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when no memory key was provided.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, KeyConstant).IsRereadRequired(out this.KeyName);
        (constants, DefaultValueConstant).IsRereadRequired(out this.DefaultValue);
        if (KeyName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "key required as primary constant"));
            return;
        }

        if (interaction.TryFindVariable(KeyName, out object? candidate) && candidate != null)
        {
            OnThen?.Invoke(this, new CommonInteraction(interaction, candidate));
        }
        else
        {
            OnElse?.Invoke(this, new CommonInteraction(interaction, DefaultValue));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => new CommonInteraction(source, ex.ToString());
}