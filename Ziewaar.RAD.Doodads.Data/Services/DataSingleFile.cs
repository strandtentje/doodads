#nullable enable
#pragma warning disable 67

using System.Collections.Generic;
using System.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Data;

[Category("Data")]
[Title("First column into list")]
[Description("""
    Works like DataSingleColumn, but instead of iterating while the query runs, it accumulates the entire
    column into a list and sticks that into Register.
    """)]
public class DataSingleFile : DataService<List<object>>
{
    protected override List<object> WorkWithCommand(IDbCommand command, IInteraction cause)
    {
        List<object> result = new();
        using (var reader = command.ExecuteReader())
        {
            result.Add(reader.GetValue(0));
        }
        return result;
    }
    protected override void FinalizeResult(List<object> output, IInteraction cause)
    {
        InvokeThen(new CommonInteraction(cause, output));
        if (output.Count == 0)
            InvokeElse(new CommonInteraction(cause, output));
    }
}
