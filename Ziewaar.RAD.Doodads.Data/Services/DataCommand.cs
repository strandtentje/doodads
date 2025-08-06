#nullable enable
#pragma warning disable 67
using System.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Data.Services;

[Category("Databases & Querying")]
[Title("Execute DB Command that affects rows")]
[Description("""
    Uses the in-context command source to execute a command that may affect rows.
    Depending on the affected rows, it may propagate differently; OnElse happens if nothing
    was affected. OnThen happens is something was affected.
    """)]
public class DataCommand : DataService<int>
{
    protected override int WorkWithCommand(IDbCommand command, IInteraction cause) => command.ExecuteNonQuery();
    protected override void FinalizeResult(int output, IInteraction cause)
    {
        switch (output)
        {
            case 0:
                InvokeElse(cause);
                break;
            default:
                InvokeThen(cause);
                break;
        }
    }
}
