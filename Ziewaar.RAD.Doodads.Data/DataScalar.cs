#nullable enable
#pragma warning disable 67

using System;
using System.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Data;

[Category("Data")]
[Title("Get the first row/first column decimal")]
[Description("""
    Executes a full query, but returns the result out of the first row and column as a decimal in register.
    """)]
public class DataScalar : DataService<object>
{
    protected override object WorkWithCommand(IDbCommand command, IInteraction cause) =>
        command.ExecuteScalar().ConvertNumericToDecimal();
    protected override void FinalizeResult(object output, IInteraction cause)
    {
        if (output == null || output == DBNull.Value)
            InvokeElse(cause);
        else
            InvokeThen(new CommonInteraction(cause, output));
    }
}
