#nullable enable
#pragma warning disable 67
using System.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Data.Services;

[Category("Databases & Querying")]
[Title("Iterate the first column of the query result")]
[Description("""
    For each result row in the query, it'll take the value of the first column, and stick
    it into Register and hit OnThen for each row.
    """)]
public class DataSingleColumn : DataService<int>
{
    protected override int WorkWithCommand(IDbCommand command, IInteraction cause)
    {
        int counter = 0;
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                InvokeThen(new CommonInteraction(cause, reader.GetValue(0)));
                counter++;
            }
        }
        return counter;
    }
    protected override void FinalizeResult(int output, IInteraction cause)
    {
        if (output == 0)
            InvokeElse(cause);
    }
}
