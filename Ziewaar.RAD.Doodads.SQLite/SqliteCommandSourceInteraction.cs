#nullable enable
#pragma warning disable 67
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.Data;

namespace Ziewaar.RAD.Doodads.SQLite;

public class SqliteCommandSourceInteraction(IInteraction interaction, ThreadLocal<ObjectPool<SqliteCommand>> currentCommands) : ICommandSourceInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public string[] DetermineParamNames(string queryText)
    {
        var prefixedQueryText = $" {queryText}";
        var splitAtParamMarker = queryText.Split('$').Skip(1).ToArray();
        List<string> paramNames = new List<string>(splitAtParamMarker.Length);
        foreach (var item in splitAtParamMarker)
        {
            char[] supposedNameCharacters = item.TakeWhile(x => char.IsLetterOrDigit(x) || x == '_').ToArray();
            paramNames.Add(new string(supposedNameCharacters));
        }
        if (paramNames.Any(x => x.Length < 1))
            throw new ArgumentOutOfRangeException("one of the parameters had no name");
        return paramNames.ToArray();
    }
    public string GenerateQueryFor(string fileName) => "";
    public string MakeFilenameSpecific(string queryTextOrFilePath)
    {
        if (!queryTextOrFilePath.EndsWith(".sqlite.sql", StringComparison.InvariantCultureIgnoreCase))
        {
            if (queryTextOrFilePath.EndsWith(".sql", StringComparison.InvariantCultureIgnoreCase))
            {
                return $"{queryTextOrFilePath.Substring(0, queryTextOrFilePath.Length - 4)}.sqlite.sql";
            }
        }
        return queryTextOrFilePath;
    }

    public TResult UseCommand<TResult>(Func<IDbCommand, TResult> commandUser)
    {
        var freshCommand = currentCommands.Value.Get();
        try
        {
            return commandUser(freshCommand);
        }
        finally
        {
            currentCommands.Value.Return(freshCommand);
        }
    }
}
