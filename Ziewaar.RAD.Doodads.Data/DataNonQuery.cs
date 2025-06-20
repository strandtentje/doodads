#nullable enable
#pragma warning disable 67

using System.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Data;

[Category("Data")]
[Title("Execute a non-query; ignore result.")]
[Description("""
    Regardless of what the query returns, or if it produces any data at all, OnThen
    will happen after the query ran.
    """)]
public class DataNonQuery : DataService<int>
{
    protected override void FinalizeResult(int output, IInteraction cause)
    {
        InvokeThen(cause);
    }

    protected override int WorkWithCommand(IDbCommand command, IInteraction cause)
    {
        return command.ExecuteNonQuery();
    }
}
