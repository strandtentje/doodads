#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Data;
public static class UpdatingValueExtensions
{
    public static bool IsRereadRequired<TResult>(
        this (StampedMap map, IUpdatingValue value) selection, out TResult? value) =>
        selection.IsRereadRequired(() => default, out value);
    public static bool IsRereadRequired<TResult>(
        this (StampedMap map, IUpdatingValue value) selection,
        Func<TResult?> sourceDefault,
        out TResult? value)
    {
        if (selection.value.IsUpdateRequiredFrom(selection.map, out var candidate))
        {
            if (candidate is TResult resultCandidate)
            {
                value = resultCandidate;
            }
            else if (sourceDefault() is { } defaultCandidate)
            {
                selection.value.UpdateInto(selection.map, defaultCandidate);
                value = defaultCandidate;
            }
            else
            {
                value = default(TResult);
            }
            return true;
        }
        else
        {
            if (candidate is TResult resultCandidate)
            {
                value = resultCandidate;
            }
            else if (sourceDefault() is { } defaultCandidate)
            {
                selection.value.UpdateInto(selection.map, defaultCandidate);
                value = defaultCandidate;
            }
            else
            {
                value = default(TResult);
            }
            return false;
        }
    }
    public static bool HasPrimary(this StampedMap map) =>
        map.PrimaryConstant is string str && !string.IsNullOrWhiteSpace(str);
}