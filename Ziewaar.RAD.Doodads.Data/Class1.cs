#nullable enable
#pragma warning disable 67

using System;
using System.Data;
using System.Globalization;
using System.IO;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Data;

public interface ICommandSourceInteraction : IInteraction
{
    TResult UseCommand<TResult>(Func<IDbCommand, TResult> commandUser);
    string[] DetermineParamNames(string queryText);
}

public class DataCommand : DataService
{

}

public abstract class DataService<TResult> : IService
{
    private readonly UpdatingPrimaryValue QueryTextOrFileConstant = new();
    private string? QueryFilePath;
    private long QueryFileAge;
    private string? QueryText;
    private string[]? ParameterNames;

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    protected abstract TResult WorkWithCommand(IDbCommand command);

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ICommandSourceInteraction>(out var commandSource) || commandSource == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "data command requires a connection source to preceede it"));
            return;
        }
        if ((constants, QueryTextOrFileConstant).IsRereadRequired(out string? queryTextOrFilePath) && queryTextOrFilePath != null)
        {
            if (queryTextOrFilePath.EndsWith(".sql", true, CultureInfo.InvariantCulture))
            {
                this.QueryFilePath = queryTextOrFilePath;
                if (!File.Exists(queryTextOrFilePath))
                    File.Create(queryTextOrFilePath).Close();
                QueryFileAge = -1;
            }
            else
            {
                this.QueryFilePath = null;
                this.QueryText = queryTextOrFilePath;
                this.ParameterNames = commandSource.DetermineParamNames(this.QueryText);
            }
        }
        if (QueryFilePath != null)
        {
            var newFileAge = File.GetLastWriteTime(QueryFilePath).ToBinary();
            if (QueryFileAge != newFileAge)
            {
                QueryFileAge = newFileAge;
                this.QueryText = File.ReadAllText(queryTextOrFilePath);
                this.ParameterNames = commandSource.DetermineParamNames(this.QueryText);
            }
        }
        commandSource.UseCommand(command =>
        {
            foreach (var item in this.ParameterNames ?? [])
            {
                var newParam = command.CreateParameter();
                newParam.ParameterName = item;
                if (interaction.TryFindVariable(item, out object? paramToIdentify))
                {
                    if (paramToIdentify == null)
                    {
                        newParam.Value = DBNull.Value;
                    }
                    else if (paramToIdentify is string textParam)
                    {
                        newParam.Value = textParam;
                        newParam.DbType = DbType.String;
                    }
                    else if (paramToIdentify is decimal dcParam)
                    {
                        newParam.Value = dcParam;
                        newParam.DbType = DbType.Decimal;
                    }
                    else if (paramToIdentify is DateTime dtParam)
                    {
                        newParam.Value = dtParam;
                        newParam.DbType = DbType.DateTime;
                    }
                    else if (paramToIdentify is TimeSpan tsParam)
                    {
                        newParam.Value = tsParam;
                        newParam.DbType = DbType.Time;
                    }
                    else if (paramToIdentify is bool blParam)
                    {
                        newParam.Value = blParam;
                        newParam.DbType = DbType.Boolean;
                    }
                    else if (paramToIdentify is byte[] bytParam)
                    {
                        newParam.Value = bytParam;
                        newParam.DbType = DbType.Binary;
                    }
                    else
                    {
                        try
                        {
                            newParam.Value = Convert.ToDecimal(paramToIdentify);
                            newParam.DbType = DbType.Decimal;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"for db param {item}", ex);
                        }
                    }
                }
                command.Parameters.Add(newParam);
            }
            command.CommandText = this.QueryText;
            return WorkWithCommand(command);
        });
    }

    public void HandleFatal(IInteraction source, Exception ex)
    {
        throw new NotImplementedException();
    }
}

public class DataQuery : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        throw new NotImplementedException();
    }

    public void HandleFatal(IInteraction source, Exception ex)
    {
        throw new NotImplementedException();
    }
}

public class DataScalar : IService
{
    public event CallForInteraction OnThen;
    public event CallForInteraction OnElse;
    public event CallForInteraction OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        throw new NotImplementedException();
    }

    public void HandleFatal(IInteraction source, Exception ex)
    {
        throw new NotImplementedException();
    }
}

public class DataSingleColumn : IService
{
    public event CallForInteraction OnThen;
    public event CallForInteraction OnElse;
    public event CallForInteraction OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        throw new NotImplementedException();
    }

    public void HandleFatal(IInteraction source, Exception ex)
    {
        throw new NotImplementedException();
    }
}
