#pragma warning disable 67
using Define.Doodads.Expo.Timeline;
using System.Collections;
using Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTests;

public class SeparatedContains : BasicService
{
    public override event CallForInteraction? OnThen;
    public override event CallForInteraction? OnElse;
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        string subVar, superVar;
        char separator;
        if (constants.PrimaryConstant != null && constants.PrimaryConstant.IsntJustAnObject() && constants.PrimaryConstant is IEnumerable ieSubSuper)
        {
            var items = ieSubSuper.OfType<object>().Where(x => x.IsntJustAnObject()).Select(x => x.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (items.Length < 2)
                throw new BasicException("when using array primary constant, expected two names; for superset, subset and optionally separator");
            superVar = items[0];
            subVar = items[1];
            if (items.Length > 2 && items[2].Length > 0)
                separator = items[2][0];
            else
                separator = ',';
        }
        else
        {
            superVar = constants.NamedItems.TryGetValue("superset", out var supersetCandidate) &&
                supersetCandidate.IsntJustAnObject() && supersetCandidate.ToString() is string supersetString &&
                !string.IsNullOrWhiteSpace(supersetString) ? supersetString : throw new BasicException("expected superset variable name");
            subVar = constants.NamedItems.TryGetValue("subset", out var subsetCandidate) &&
                subsetCandidate.IsntJustAnObject() && subsetCandidate.ToString() is string subsetString &&
                !string.IsNullOrWhiteSpace(subsetString) ? subsetString : throw new BasicException("expected subset variable name");
            separator = constants.NamedItems.TryGetValue("separator", out var separatorCandidate) &&
                separatorCandidate.IsntJustAnObject() && separatorCandidate.ToString() is string separatorString &&
                !string.IsNullOrWhiteSpace(separatorString) && separatorString.Length == 1 ? separatorString[0] : throw new BasicException("expected single character separator");
        }

        if (!interaction.TryFindVariable(superVar, out object? superObject) || superObject == null
            || !superObject.IsntJustAnObject() || superObject.ToString() is not string superString ||
            string.IsNullOrWhiteSpace(superString))
            throw new BasicException("no super variable found");
        if (!interaction.TryFindVariable(subVar, out object? subObject) || subObject == null
            || !subObject.IsntJustAnObject() || subObject.ToString() is not string subString ||
            string.IsNullOrWhiteSpace(subString))
            throw new BasicException("no sub variable found");

        var superParts = new SortedSet<string>(
            superString.Split([separator], StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray(),
            StringComparer.OrdinalIgnoreCase);

        if (superParts.Contains(subString))
        {
            superParts.Remove(subString);
            OnThen?.Invoke(this, interaction.AppendRegister(string.Join(separator.ToString(), superParts)));
        }
        else
        {
            superParts.Add(subString);
            OnElse?.Invoke(this, interaction.AppendRegister(string.Join(separator.ToString(), superParts)));
        }
    }
}