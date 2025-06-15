namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

public static class SortedListExtensions
{
    public static TResult InsertIgnore<TResult>(this SortedList<string, object> list, string key, TResult defaultValue = default)
    {
        if (!list.TryGetValue(key, out var candidateObjectValue))
            list.Add(key, candidateObjectValue = defaultValue);
        if (candidateObjectValue is not TResult candidateResultValue) 
            list[key] = candidateResultValue = defaultValue;
        return candidateResultValue;
    }
    public static SortedList<string, object> ToSortedList(this Exception ex, bool includeStack = true, bool includeData = true, bool includeInner = true)
    {
        var result = new SortedList<string, object>
        {
            ["Type"] = ex.GetType().FullName,
            ["Message"] = ex.Message,
            ["Source"] = ex.Source,
            ["HResult"] = ex.HResult,
            ["TargetSite"] = ex.TargetSite?.ToString()
        };

        if (includeStack && !string.IsNullOrEmpty(ex.StackTrace))
            result["StackTrace"] = ex.StackTrace;

        if (includeData && ex.Data?.Count > 0)
        {
            var dataDict = new Dictionary<string, object>();
            foreach (var key in ex.Data.Keys)
            {
                if (key is string strKey)
                    dataDict[strKey] = ex.Data[key];
                else
                    dataDict[key?.ToString() ?? "<null>"] = ex.Data[key];
            }
            result["Data"] = dataDict;
        }

        if (includeInner && ex.InnerException != null)
        {
            result["InnerException"] = ex.InnerException.ToSortedList(includeStack, includeData, includeInner);
        }

        return result;
    }
}
