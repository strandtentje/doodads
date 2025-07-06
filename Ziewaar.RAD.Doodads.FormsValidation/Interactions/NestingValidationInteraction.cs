using Ziewaar.RAD.Doodads.FormsValidation.Common;

namespace Ziewaar.RAD.Doodads.FormsValidation.Interactions;
#pragma warning disable 67
public class NestingValidationInteraction(IInteraction interaction, object registerValue)
    : CommonInteraction(interaction, register: registerValue)
{
    public Tristate Validity = Tristate.Unknown;
}