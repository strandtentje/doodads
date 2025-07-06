#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ziewaar.RAD.Doodads.Data.Implementable.Support;
#pragma warning disable 67
public class DefaultCommandTextPreprocessor(string dbShorthand, char paramPrefix) : ICommandTextPreprocessor
{
    public string GenerateQueryFor(string fileName) => "";
    public string MakeFilenameSpecific(string queryTextOrFilePath)
    {
        if (!queryTextOrFilePath.EndsWith($".{dbShorthand}.sql", StringComparison.InvariantCultureIgnoreCase))
        {
            if (queryTextOrFilePath.EndsWith(".sql", StringComparison.InvariantCultureIgnoreCase))
            {
                return $"{queryTextOrFilePath.Substring(0, queryTextOrFilePath.Length - 4)}.{dbShorthand}.sql";
            }
        }
        return queryTextOrFilePath;
    }
    public string[] DetermineParamNames(string queryText)
    {
        var prefixedQueryText = $" {queryText}";
        var splitAtParamMarker = queryText.Split(paramPrefix).Skip(1).ToArray();
        List<string> paramNames = new List<string>(splitAtParamMarker.Length);
        foreach (var item in splitAtParamMarker)
        {
            char[] supposedNameCharacters = item.TakeWhile(x => char.IsLetterOrDigit(x) || x == '_').ToArray();
            paramNames.Add(new string(supposedNameCharacters));
        }
        if (paramNames.Any(x => x.Length < 1))
            throw new ArgumentOutOfRangeException("one of the parameters had no name");
        return paramNames.ToArray();
    }
}