#pragma warning disable 67
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Data.Services;

public abstract class DataService<TResult> : IService
{
    [PrimarySetting("Query text or filename of query, or transaction name")]
    private readonly UpdatingPrimaryValue StringParameterConstant = new();

    private string? QueryFilePath;
    private long QueryFileAge;
    private string? QueryText;
    private string[]? ParameterNames;
    private bool AllowNull;

    [EventOccasion("When the query ran successfully or has a result")]
    public event CallForInteraction? OnThen;

    [EventOccasion("After the query ran, or when it had no output")]
    public event CallForInteraction? OnElse;

    [EventOccasion("Likely when the query text was wrong.")]
    public event CallForInteraction? OnException;

    public enum CommonBranchName
    {
        None,
        OnThen,
        OnElse,
        OnException
    };

    protected abstract TResult WorkWithCommand(IDbCommand command, IInteraction cause);
    protected abstract void FinalizeResult(TResult output, IInteraction cause);

    public virtual void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ICommandSourceInteraction>(out var commandSource) || commandSource == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "data command requires a connection source to preceede it"));
            return;
        }

        this.AllowNull = constants.NamedItems.TryGetValue("allownull", out var value) && Convert.ToBoolean(value);

        if ((constants, StringParameterConstant).IsRereadRequired(out object? stringParameterObject) &&
            stringParameterObject?.ToString() is string stringParameter)
            ProcessStringParameter(stringParameter, commandSource);
        if (FindErrorsWithQueryText(interaction, commandSource)) return;
        var output = commandSource.UseCommand(command =>
        {
            foreach (var item in this.ParameterNames ?? [])
            {
                var newParam = GenerateParameter(interaction, command, item);
                command.Parameters.Add(newParam);
            }

            command.CommandText = this.QueryText;
            var replacements = interaction.GetAllOf<QueryPlaceholderInteraction>();
            foreach (var replacement in replacements)
                command.CommandText = replacement.Apply(command.CommandText ?? "");

            return WorkWithCommand(command, interaction);
        });

        FinalizeResult(output, interaction);
    }

    private IDbDataParameter GenerateParameter(IInteraction interaction, IDbCommand command, string item)
    {
        var newParam = command.CreateParameter();
        newParam.ParameterName = item;
        if (interaction.TryFindVariable(item, out object? paramToIdentify) &&
            paramToIdentify != null)
        {
            if (FrameworkTypeAdaptorRepository.Instance.TryConvert(paramToIdentify, out var converted))
                paramToIdentify = converted;

            switch (paramToIdentify)
            {
                case string textParam:
                    newParam.Value = textParam;
                    newParam.DbType = DbType.String;
                    break;
                case decimal dcParam:
                    newParam.Value = dcParam;
                    newParam.DbType = DbType.Decimal;
                    break;
                case DateTime dtParam:
                    newParam.Value = dtParam;
                    newParam.DbType = DbType.DateTime;
                    break;
                case TimeSpan tsParam:
                    newParam.Value = tsParam;
                    newParam.DbType = DbType.Time;
                    break;
                case bool blParam:
                    newParam.Value = blParam;
                    newParam.DbType = DbType.Boolean;
                    break;
                case byte[] bytParam:
                    newParam.Value = bytParam;
                    newParam.DbType = DbType.Binary;
                    break;
                default:
                    try
                    {
                        newParam.Value = Convert.ToDecimal(paramToIdentify);
                        newParam.DbType = DbType.Decimal;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"for db param {item}", ex);
                    }

                    break;
            }
        }
        else
        {
            newParam.Value = DBNull.Value;
            if (!AllowNull)
                OnException?.Invoke(this,
                    new CommonInteraction(interaction, $"missing param {item} so setting null. query might fail."));
        }

        return newParam;
    }

    private bool FindErrorsWithQueryText(IInteraction interaction, ICommandSourceInteraction commandSource)
    {
        if (QueryFilePath != null)
        {
            var newFileAge = File.GetLastWriteTime(QueryFilePath).ToBinary();
            if (QueryFileAge != newFileAge)
            {
                QueryFileAge = newFileAge;
                this.QueryText = File.ReadAllText(QueryFilePath);
                this.ParameterNames = commandSource.DetermineParamNames(this.QueryText).Distinct().ToArray();
            }
        }

        if (string.IsNullOrWhiteSpace(this.QueryText))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "cannot execute empty query"));
            return true;
        }

        return false;
    }

    protected virtual void ProcessStringParameter(string stringParameter, ICommandSourceInteraction commandSource)
    {
        if (stringParameter.EndsWith(".sql", true, CultureInfo.InvariantCulture))
        {
            this.QueryFilePath = commandSource.MakeFilenameSpecific(stringParameter);
            if (!File.Exists(QueryFilePath))
            {
                if (File.Exists(stringParameter))
                {
                    File.Copy(stringParameter, QueryFilePath);
                }
                else
                {
                    using (var f = File.CreateText(QueryFilePath))
                    {
                        f.Write(commandSource.GenerateQueryFor(Path.GetFileName(QueryFilePath)));
                    }
                }
            }

            QueryFileAge = -1;
        }
        else
        {
            this.QueryFilePath = null;
            this.QueryText = stringParameter;
            this.ParameterNames = commandSource.DetermineParamNames(this.QueryText).Distinct().ToArray();
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    protected void InvokeThen(IInteraction source) => OnThen?.Invoke(this, source);
    protected void InvokeElse(IInteraction source) => OnElse?.Invoke(this, source);
    protected void InvokeException(IInteraction source) => OnException?.Invoke(this, source);
}