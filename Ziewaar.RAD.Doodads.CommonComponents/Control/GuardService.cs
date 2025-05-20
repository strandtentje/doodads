using System.Text.RegularExpressions;
using Ziewaar.RAD.Doodads.CoreLibrary.Attributes;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

class GuardService : IService
{
    [NamedBranch]
    public event EventHandler<IInteraction> OnError;
    [NamedBranch]
    public event EventHandler<IInteraction> Continue;
    [NamedBranch]
    public event EventHandler<IInteraction> NotApplicable;

    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        var sourceName = serviceConstants.InsertIgnore("variable", "variable");
        var groupName = serviceConstants.InsertIgnore("groupvarformat", "group_{0}");
        var regexEnable = bool.TryParse(serviceConstants.InsertIgnore("regexenabled", "false"), out bool isEnabled) && isEnabled;

        if (serviceConstants.TryGetValue("equals", out var equals) && equals is string validEquals)
        {
            if (interaction.TryFindVariable<string>(sourceName, out var candidateMatch) && candidateMatch == validEquals)
            {
                Continue?.Invoke(this, interaction);
                return;
            } else
            {
                NotApplicable?.Invoke(this, interaction);
                return;
            }
        } else if (regexEnable)
        {
            var pattern = serviceConstants.InsertIgnore("pattern", "guard_*");
            var converted = pattern.Replace("*", "(.*)").Replace("?", "(.)");
            var captured = $"^{converted}$";
            var regexPattern = serviceConstants.InsertIgnore("regex", captured);
            var compiled = new Regex(regexPattern);

            if (interaction.TryFindVariable<string>(sourceName, out var candidateMatch) && candidateMatch is string candidateString)
            {
                var matches = compiled.Matches(candidateString);
                if (matches.Count == 1)
                {
                    var builder = VariablesInteraction.CreateBuilder(interaction);
                    for (int i = 0; i < matches[0].Groups.Count; i++)
                    {
                        builder.Add(string.Format(groupName, i), matches[0].Groups[i].Value);
                    }
                    Continue?.Invoke(this, builder.Build());
                    return;
                } else
                {
                    NotApplicable?.Invoke(this, interaction);
                    return;
                }
            }
        } else
        {
            OnError?.Invoke(this, VariablesInteraction.ForError(interaction, "Guard not configured"));
        }
    }
}
