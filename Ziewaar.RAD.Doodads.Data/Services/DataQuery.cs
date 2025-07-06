#nullable enable
#pragma warning disable 67
using System.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Data.Services;

[Category("Data")]
[Title("Cursor through query results")]
[Description("""
    Execute a query, and after each result row was retrieved, invoke OnThen.
    """)]
public class DataQuery : DataService<object?>
{
    protected override object? WorkWithCommand(IDbCommand command, IInteraction cause)
    {
        using (var reader = command.ExecuteReader())
        {
            var dqr = new DataQueryInteraction(cause, reader);
            while(reader.Read())
            {
                InvokeThen(dqr);
            }
        }
        return null;
    }
    protected override void FinalizeResult(object? output, IInteraction cause)
    {
        InvokeElse(cause);
    }
}
