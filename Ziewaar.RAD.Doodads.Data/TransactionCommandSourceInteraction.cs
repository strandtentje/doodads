#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Data.Services;
#pragma warning disable 67
public class TransactionCommandSourceInteraction(
    IInteraction parent,
    ICommandSourceInteraction csi,
    IDbTransaction tx,
    string transactionName)
    : ICommandSourceInteraction
{
    public string TransactionName => transactionName;
    private bool IsUsable = true;
    private object _commitLock = new();
    private bool IsCommitted = false;
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public TResult UseCommand<TResult>(Func<IDbCommand, TResult> commandUser)
    {
        lock (_commitLock)
        {
            if (!IsUsable) throw new Exception("Transaction already done.");
        }
        return csi.UseCommand(x =>
        {
            x.Transaction = tx;
            return commandUser(x);
        });
    }
    public string[] DetermineParamNames(string queryText) => csi.DetermineParamNames(queryText);
    public string GenerateQueryFor(string fileName) => csi.GenerateQueryFor(fileName);
    public string MakeFilenameSpecific(string queryTextOrFilePath) => csi.MakeFilenameSpecific(queryTextOrFilePath);
    public void SetCommitted()
    {
        lock (_commitLock)
        {
            IsUsable = false;
            if (IsCommitted) throw new Exception($"Already committed {transactionName}");
            IsCommitted = true;
            tx.Commit();
        }
    }
    public ICommandSourceInteraction RestoreRegularCommands() => new RestoredCommandSourceInteraction(csi);
    public bool TryRollback()
    {
        lock (_commitLock)
        {
            IsUsable = false;
            if (!IsCommitted)
            {
                tx.Rollback();
                return true;
            }
            return false;
        }
    }
}