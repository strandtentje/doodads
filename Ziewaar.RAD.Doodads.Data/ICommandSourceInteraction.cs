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
public static class DbTypeExtensions
{
    public static (DbType dbType, string name, object value) ToParam(this DbType dbtype, string name, object value) =>
        (dbtype, name, value);
    public static IDbCommand SetParams(this IDbCommand command,
        params (DbType dbType, string name, object value)[] parameters)
    {
        command.Parameters.Clear();
        foreach (var valueTuple in parameters)
        {
            var param = command.CreateParameter();
            param.DbType  = valueTuple.dbType;
            param.ParameterName = valueTuple.name;
            param.Value = valueTuple.value;
            command.Parameters.Add(param);
        }
        return command;
    }
}