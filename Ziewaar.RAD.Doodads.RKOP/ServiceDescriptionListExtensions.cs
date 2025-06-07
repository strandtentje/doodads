using System.Collections.Generic;


namespace Ziewaar.RAD.Doodads.RKOP;

public static class ServiceDescriptionListExtensions
{
    public static SortedList<string, TResult> ToSortedList<TResult>(
        this List<ServiceDescription<TResult>> descriptions)
        where TResult : class, IInstanceWrapper, new()
    {
        SortedList<string, TResult> results = [];
        foreach (var item in descriptions)
            results[item.ConstantsDescription.BranchKey] = item.ResultSink;
        return results;
    }
}
