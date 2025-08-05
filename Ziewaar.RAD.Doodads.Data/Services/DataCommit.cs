#nullable enable
using System;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Data.Services;
#pragma warning disable 67
[Category("Data")]
[Title("Commit a transaction")]
[Description("Commits a transaction provided a fixed name. ")]
public class DataCommit : IService
{
    [PrimarySetting("Set a name for this transaction to match")]
    private readonly UpdatingPrimaryValue TransactionNameConstant = new();
    private string? TransactionName;
    [EventOccasion("When the transaction was committed; commands here after will not be in a transaction.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when the transaction was committed already, or none was found with our name.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, TransactionNameConstant).IsRereadRequired(out string? candidateTransactionName))
            this.TransactionName = candidateTransactionName;
        if (!TryValidateTransactionName(interaction)) return;
        if (interaction.TryGetClosest(out TransactionCommandSourceInteraction? tcsi,
                x => x.TransactionName == this.TransactionName) && tcsi != null)
        {
            tcsi.SetCommitted();
            OnThen?.Invoke(this, tcsi.RestoreRegularCommands());
        }
        else
            OnException?.Invoke(this, new CommonInteraction(interaction, "No transaction with our name was found."));
    }
    private bool TryValidateTransactionName(IInteraction interaction)
    {
        if (!string.IsNullOrWhiteSpace(TransactionName))
            return true;
        OnException?.Invoke(this,
            new CommonInteraction(interaction,
                "Command source is required for opening a transaction."));
        return false;
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}