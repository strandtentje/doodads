#pragma warning disable 67
#nullable enable

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
[Category("Call Definition Return")]
[Title("Call to Definition in File")]
[Shorthand("<<CONSTANTS>>")]
[Description("""
    One of multiple root services in an rkop file; make sure each Definition name in a file 
    is unique and descriptive. No programming casing is needed, so where you would normally do 
    something like `bakeCookies(temp=100)` or `BakeCookies(100)`, you are encouraged to say 
    `Definition("Bake Cookies at 100 degrees")` - rkop has no comments in its syntax. It is 
    encouraged to instead title Definitions consistently and clearly.
    """)]
public class Definition : IService
{
    [PrimarySetting("Name of definition Call can reach out to.")]
    private readonly UpdatingPrimaryValue DefinitionNameConst = new();
    [EventOccasion("When the Definition was Called for")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when Definition wasn't the first thing after a call")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction is CallingInteraction ci)
        {
            OnThen?.Invoke(this, new DefaultValueInteraction(interaction, memory: constants.ToSortedList()));
        }
        else if (interaction is ISelfStartingInteraction ss)
        {
            OnThen?.Invoke(this, new DefaultValueInteraction(interaction, memory: constants.ToSortedList()));
        }
        else
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    register: "Warning: should not be used in the middle of a program."));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
