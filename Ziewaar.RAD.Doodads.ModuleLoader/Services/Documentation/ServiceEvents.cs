#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Documentation;
#pragma warning disable 67
[Category("Reflection & Documentation")]
[Title("Enumerate events that service exposes")]
[Description("""
             Iterate event names of the service name currently in register, while
             an appropriate continue is triggered.
             """)]
public class ServiceEvents : IteratingService
{
    protected override bool RunElse { get; }
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (repeater.Register is not string serviceName)
            throw new Exception("Service name required in register");
        return DocumentationRepository.Instance.GetTypeEvents(serviceName).Select(repeater.AppendRegister);
    }
}