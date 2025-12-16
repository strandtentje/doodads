#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Documentation;
#pragma warning disable 67
[Category("Reflection & Documentation")]
[Title("Enumerate settings that the service exposes.")]
[Description("""
             Provided a service name in register, loop through
             the settings it exposes.
             """)]
public class ServiceSettings : IteratingService
{
    protected override bool RunElse { get; }
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (repeater.Register is not string serviceName)
            throw new Exception("Service name required in register");
        return DocumentationRepository.Instance.GetTypeNamedSettings(serviceName).Select(repeater.AppendRegister);
    }
}