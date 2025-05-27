using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

public static class MultipleSourceExtensions
{
    public static string SourceSetting(
        this (IService sender, ServiceConstants serviceConstants, IInteraction interaction, EventHandler<IInteraction> forwardSourcing) 
        context, string name, string fallback)
    {
        (var sender, var serviceConstants, var interaction, var forwardSourcing) = context;
        if (forwardSourcing != null)
        {
            var getter = new RawStringSinkingInteraction(interaction);
            forwardSourcing.Invoke(sender, getter);
            return getter.GetFullString();
        }
        else if (serviceConstants.TryGetValue($"{name}_variable", out var candidateValue) &&
            candidateValue is string candidateString)
        {
            return candidateString;
        }
        else
        {
            return serviceConstants.InsertIgnore(name, fallback);
        }
    }
}
