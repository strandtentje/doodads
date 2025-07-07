#nullable enable
#pragma warning disable 67
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Data.Services;

[Category("Data")]
[Title("Cursor through query results after the query completes")]
[Description("""
    Retrieve all result rows then iterate through them.
    """)]
public class BufferedDataQuery : DataService<object?>
{
    protected override object? WorkWithCommand(IDbCommand command, IInteraction cause)
    {
        List<IReadOnlyDictionary<string, object>> results = new();
        using (var reader = command.ExecuteReader())
        {
            var dqr = new DataQueryInteraction(cause, reader);
            while (reader.Read())
            {
                results.Add(dqr.Memory.ToDictionary(x => x.Key, x => x.Value));
            }
        }
        foreach (var item in results)
        {
            InvokeThen(new CommonInteraction(cause, memory: item));
        }
        return results.Count;
    }
    protected override void FinalizeResult(object? output, IInteraction cause)
    {
        if (output is not int numb || numb == 0)
            InvokeElse(cause);
    }
}

