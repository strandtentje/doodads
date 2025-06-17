#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Reflection;
[Category("Reflection")]
[Title("Get value of a service constant")]
[Description("""
             Provided a constname and service info under expression, will attempt to put the configured 
             settings value into the register. this either works with a key string in register, and expression
             in memory, or expression in register, and key string in memory.
             """)]
public class GetServiceConst : IService
{
    [EventOccasion("When a value was found and put into register")]
    public event CallForInteraction? OnThen;
    [EventOccasion("when the const wasn't assigned.")]
    public event CallForInteraction? OnElse;
    [EventOccasion(
        "When `expression` and `constname` weren't found (they cant both come from memory)")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        ServiceDescription<ServiceBuilder> description;
        string constName;
        if (interaction.Register is ServiceDescription<ServiceBuilder> descriptionFromRegister &&
            interaction.TryFindVariable<string>("constname", out string? constNameFromMemory) &&
            constNameFromMemory != null)
        {
            description = descriptionFromRegister;
            constName = constNameFromMemory;
        }
        else if (interaction.Register is string constNameFromRegister &&
                 interaction.TryFindVariable<ServiceDescription<ServiceBuilder>>("expression",
                     out var descriptionFromMemory) && descriptionFromMemory != null)
        {
            constName = constNameFromRegister;
            description = descriptionFromMemory;
        }
        else
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "expected service description and const name via register or memory at `constname` and `expression`"));
            return;
        }

        var value = description.Constructor.Constants.Members.SingleOrDefault(x => x.Key == constName);
        if (value == null)
        {
            OnElse?.Invoke(this,
                new CommonInteraction(interaction,
                    "const did not exist at this service under this name"));
            return;
        }

        OnThen?.Invoke(this, new CommonInteraction(interaction, value.Value.GetValue()));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}