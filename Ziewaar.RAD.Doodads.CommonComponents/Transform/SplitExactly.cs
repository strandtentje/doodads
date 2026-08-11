using Define.Doodads.Expo.Timeline;
using System.Collections;
using Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;

public class SplitExactly : BasicService
{
    private const string DEFAULT = "def";

    public override event CallForInteraction? OnThen;
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        string[] names;
        if (!constants.PrimaryConstant.IsntJustAnObject())
            throw new BasicException("name array reqd as primary constant");
        else if (constants.PrimaryConstant is string)
            throw new BasicException("more than one name reqd as primary constant");
        else if (constants.PrimaryConstant is not IEnumerable namesEnumerable)
            throw new BasicException("name array reqd as primary constant; something else given.");
        else
            names = namesEnumerable.OfType<object>().Where(x => !x.IsntJustAnObject()).Select(x => x.ToString()).ToArray();

        var splitter = constants.NamedItems.TryGetValue("splitter", out var splitterCandidate)
            && splitterCandidate is string stringSplitter ? stringSplitter.ElementAtOrDefault(0) : ' ';
        if (splitter == default(char)) splitter = ' ';

        var replacements = constants.NamedItems.
            Where(x => x.Key.StartsWith(DEFAULT)).
            ToDictionary(x => x.Key.Substring(DEFAULT.Length), x => x.Value);

        var maxItems = names.Length;

        var toSplit = interaction.Register.ToString();
        if (string.IsNullOrWhiteSpace(toSplit))
            throw new BasicException("string reqd as input");
        var splitValues = toSplit.Split([splitter], maxItems);

        OnThen?.Invoke(this, new CommonInteraction(interaction, memory: new SplitExactlyMemory(names, replacements, splitValues)));
    }
}
