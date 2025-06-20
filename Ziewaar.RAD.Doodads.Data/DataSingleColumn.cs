#nullable enable
#pragma warning disable 67

using System.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Data;

[Category("Data")]
[Title("Iterate the first column of the query result")]
[Description("""
    For each result row in the query, it'll take the value of the first column, and stick
    it into Register and hit OnThen for each row.
    """)]
public class DataSingleColumn : DataService<int>
{
    protected override int WorkWithCommand(IDbCommand command, IInteraction cause)
    {
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                InvokeThen(new CommonInteraction(cause, reader.GetValue(0)));
            }
        }
        return 0;
    }
    protected override void FinalizeResult(int output, IInteraction cause)
    {
        InvokeElse(cause);
    }
}
