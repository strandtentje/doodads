#nullable enable
#pragma warning disable 67
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Data.Services;

[Category("Databases & Querying")]
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

        for (var index = 0; index < results.Count; index++)
        {
            var item = results[index];
            InvokeThen(new BufferedDataInteraction(cause, item, index, results.Count));
        }

        return results.Count;
    }

    protected override void FinalizeResult(object? output, IInteraction cause)
    {
        if (output is not int numb || numb == 0)
            InvokeElse(cause);
    }
}