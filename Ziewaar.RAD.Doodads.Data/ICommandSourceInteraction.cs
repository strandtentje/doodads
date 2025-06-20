#nullable enable
#pragma warning disable 67

using System;
using System.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Data;

public interface ICommandSourceInteraction : IInteraction
{
    TResult UseCommand<TResult>(Func<IDbCommand, TResult> commandUser);
    string[] DetermineParamNames(string queryText);
    string GenerateQueryFor(string fileName);
    string MakeFilenameSpecific(string queryTextOrFilePath);
}
