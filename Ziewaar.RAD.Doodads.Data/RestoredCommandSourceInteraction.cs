#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Data.Services;
#pragma warning disable 67
public class RestoredCommandSourceInteraction(ICommandSourceInteraction csi) : ICommandSourceInteraction
{
    public IInteraction Stack => csi.Stack;
    public object Register => csi.Register;
    public IReadOnlyDictionary<string, object> Memory => csi.Memory;
    public TResult UseCommand<TResult>(Func<IDbCommand, TResult> commandUser) => csi.UseCommand(commandUser);
    public string[] DetermineParamNames(string queryText) => csi.DetermineParamNames(queryText);
    public string GenerateQueryFor(string fileName) => csi.GenerateQueryFor(fileName);
    public string MakeFilenameSpecific(string queryTextOrFilePath) => csi.MakeFilenameSpecific(queryTextOrFilePath);
}