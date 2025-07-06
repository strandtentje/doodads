namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.FormBuilder;
#pragma warning disable 67
public class NestingValidationInteraction(IInteraction interaction, object registerValue)
    : CommonInteraction(interaction, register: registerValue)
{
    public Tristate Validity = Tristate.Unknown;
}