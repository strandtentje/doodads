#nullable enable
#pragma warning disable 67
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Data.Services;
[Category("Databases & Querying")]
[Title("Read a row")]
[Description("Gets a data row when it's present. Otherwise, OnElse happens.")]
public class DataRow : DataService<object>
{
    protected override object WorkWithCommand(IDbCommand command, IInteraction cause)
    {
        using (var reader = command.ExecuteReader())
        {
            if (reader.Read())
                return new DataQueryInteraction(cause, reader).Memory.ToDictionary(x => x.Key, x => x.Value);
            else
                return cause;
        }
    }
    protected override void FinalizeResult(object output, IInteraction cause)
    {
        if (output is IReadOnlyDictionary<string, object> results)
            InvokeThen(new CommonInteraction(cause, results));
        else if (output is IInteraction emptyResult)
            InvokeElse(emptyResult);
        else
            InvokeException(new CommonInteraction(cause, "Unknown data result"));
    }
}