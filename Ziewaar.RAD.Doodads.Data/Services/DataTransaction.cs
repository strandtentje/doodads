#nullable enable
using System;
using System.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Data.Services;
#pragma warning disable 67
[Category("Databases & Querying")]
[Title("Start a transaction")]
[Description("Opens a transaction and commits it with DataCommit. If not committed, a rollback will happen.")]
public class DataTransaction : IService
{
    [PrimarySetting("Set a name for this transaction to match the one in the commit")]
    private readonly UpdatingPrimaryValue TransactionNameConstant = new();
    private string? TransactionName;
    [EventOccasion("When the transaction has opened, it is available for querying against here.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the transaction was not committed")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when no name was set, no connection was present or nesting transactions were attempted")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, TransactionNameConstant).IsRereadRequired(out string? candidateTransactionName))
            this.TransactionName = candidateTransactionName;
        if (!TryValidateTransactionName(interaction)) return;
        if (!TryGetCleanCommandSource(interaction, out var csi)) return;
        if (!TryGetTransactionFrom(csi!, out var tx)) return;
        var txi = new TransactionCommandSourceInteraction(interaction, csi!, tx!, TransactionName!);
        try
        {
            OnThen?.Invoke(this, txi);
        }
        finally
        {
            if (txi.TryRollback())
            {
                OnElse?.Invoke(this, interaction);
            }
        }
    }
    private bool TryValidateTransactionName(IInteraction interaction)
    {
        if (string.IsNullOrWhiteSpace(TransactionName))
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "Transaction name is required for opening a transaction."));
            return false;
        }
        else
        {
            return true;
        }
    }
    private bool TryGetTransactionFrom(ICommandSourceInteraction commandSourceInteraction,
        out IDbTransaction? dbTransaction)
    {
        dbTransaction =
            commandSourceInteraction.UseCommand(x => x.Transaction == null ? x.Connection.BeginTransaction() : null);
        if (dbTransaction == null)
            OnException?.Invoke(this,
                new CommonInteraction(commandSourceInteraction, "Command was in transaction, but not due to us."));
        return dbTransaction != null;
    }
    private bool TryGetCleanCommandSource(IInteraction interaction,
        out ICommandSourceInteraction? commandSourceInteraction)
    {
        if (!interaction.TryGetClosest<ICommandSourceInteraction>(out commandSourceInteraction) || commandSourceInteraction == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "Command source is required for opening a transaction."));
            return false;
        }
        else if (commandSourceInteraction is TransactionCommandSourceInteraction)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "Transactions may not be nested"));
            return false;
        }
        else
        {
            return true;
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}