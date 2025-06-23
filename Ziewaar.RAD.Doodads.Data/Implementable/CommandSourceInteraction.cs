#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Microsoft.Extensions.ObjectPool;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.Data;

namespace Ziewaar.RAD.Doodads.SQLite;
#pragma warning disable 67
public class CommandSourceInteraction<TCommand>(
    IInteraction parent,
    ThreadLocal<ObjectPool<TCommand>> commandPool,
    ICommandTextPreprocessor textPreprocessor) : ICommandSourceInteraction
    where TCommand : class, IDbCommand
{
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public TResult UseCommand<TResult>(Func<IDbCommand, TResult> commandUser)
    {
        var freshCommand = commandPool.Value.Get();
        try
        {
            return commandUser(freshCommand);
        }
        finally
        {
            commandPool.Value.Return(freshCommand);
        }
    }
    public string[] DetermineParamNames(string queryText) => textPreprocessor.DetermineParamNames(queryText);
    public string GenerateQueryFor(string fileName) => textPreprocessor.GenerateQueryFor(fileName);
    public string MakeFilenameSpecific(string queryTextOrFilePath) =>
        textPreprocessor.MakeFilenameSpecific(queryTextOrFilePath);
}