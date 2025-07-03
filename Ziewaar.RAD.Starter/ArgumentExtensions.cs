#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Ziewaar.RAD.Starter;
public static class ArgumentExtensions
{
    public static string[] ToFilenameFlags<TInput>(
        this TInput item,
        string prefix,
        Func<TInput, string>? selectValue = null) =>
        ToMultipleFilenameFlags([item], prefix, selectValue);
    public static string[] ToMultipleFilenameFlags<TInput>(
        this IReadOnlyList<TInput> collection,
        string prefix,
        Func<TInput, string>? selectValue = null)
    {
        selectValue ??= x => x?.ToString() ?? "";
        if (collection.Count == 1)
        {
            return [$"-{prefix}", selectValue(collection[0]).PutInQuotes()];
        }

        string[] result = new string[collection.Count * 2];
        for (int i = 0; i < collection.Count; i++)
        {
            result[i * 2] = $"-{prefix}{i:0000}";
            result[i * 2 + 1] = selectValue(collection[i]).PutInQuotes();
        }
        return result;
    }
    public static string PutInQuotes(this string input) => string.Format("{0}{1}{2}", @"""", input.Replace("\\","\\\\").Replace("\"", @"\"""), @"""");
    public static TResult[] GetValuesStartingWith<TResult>(this SortedList<string, object> values, string startingWith) =>
        [.. values.Where(x => x.Key.StartsWith(startingWith, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).OfType<TResult>()];
    public static SortedList<string,object> RemoveValuesStartingWith(this SortedList<string, object> values, string startingWith)
    {
        if (values.Keys.FirstOrDefault(x => x.StartsWith(startingWith, StringComparison.OrdinalIgnoreCase)) is string illegalKey)
        {
            values.Remove(illegalKey);
            values.RemoveValuesStartingWith(startingWith);
        }
        return values;
    }
    public static SortedList<string, object> RemoveValuesStartingWithAny(this SortedList<string, object> values, params string[] startingWith)
    {
        foreach (var item in startingWith)
        {
            values.RemoveValuesStartingWith(item);
        }
        return values;
    }
    public static string TruncatePathStart(this string path, string prefix)
    {
        prefix = prefix.TrimEnd(Path.DirectorySeparatorChar);
        return path.StartsWith(prefix)
        ? path.Substring(prefix.Length).TrimStart(Path.DirectorySeparatorChar)
        : path;
    }

    public static string QualifyFullPath(this string path, string prefix) =>
        (Path.IsPathRooted(path) ? path :
        Path.Combine(prefix, path.TruncatePathStart(prefix))).Replace(@"\\", @"\");
}
