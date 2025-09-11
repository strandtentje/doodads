namespace Ziewaar.RAD.Doodads.FormsValidation.Interactions;
#pragma warning disable 67
public class NestingValidationInteraction(IInteraction interaction, object registerValue)
    : CommonInteraction(interaction, register: registerValue)
{
    public Tristate AndValidity = Tristate.Unknown;
    public Tristate OrValidity = Tristate.Unknown;
}