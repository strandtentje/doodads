#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
[Category("Reflection & Documentation")]
[Title("Get child service information from service branches")]
[Description("""
             Provided a childname and service info under expression, will attempt to put the child service
             expression into the register. this either works with a key string in register, and expression
             in memory, or expression in register, and key string in memory.
             """)]
public class GetServiceChild : IService
{
    [EventOccasion("When child service information was found and put into register")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When no child was found under that name")]
    public event CallForInteraction? OnElse;
    [EventOccasion("When `childname` or `expression` couldn't be determined")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        ServiceDescription<ServiceBuilder> description;
        string branchName;
        if (interaction.Register is ServiceDescription<ServiceBuilder> descriptionFromRegister &&
            interaction.TryFindVariable<string>("childname", out string? branchNameFromMemory) &&
            branchNameFromMemory != null)
        {
            description = descriptionFromRegister;
            branchName = branchNameFromMemory;
        }
        else if (interaction.Register is string branchNameFromRegister &&
                 interaction.TryFindVariable<ServiceDescription<ServiceBuilder>>("expression",
                     out var descriptionFromMemory) && descriptionFromMemory != null)
        {
            branchName = branchNameFromRegister;
            description = descriptionFromMemory;
        }
        else
        {
            OnElse?.Invoke(this,
                new CommonInteraction(interaction,
                    "expected service description and child name via register or memory at `childname` and `expression`"));
            return;
        }
        if (description.Children.Branches == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "no branches on this service."));
            return;
        }

        var value = description.Children.Branches.SingleOrDefault(x => x.key == branchName).value;
        OnThen?.Invoke(this, new CommonInteraction(interaction, value));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}