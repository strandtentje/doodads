#nullable enable
#pragma warning disable 67
using Microsoft.Extensions.ObjectPool;
using System.Data.SQLite;
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Data.Implementable;
using Ziewaar.RAD.Doodads.Data.Implementable.Support;

namespace Ziewaar.RAD.Doodads.SQLite;

[Category("Databases & Querying")]
[Title("Connect to a Sqlite File")]
[Description("""
    Open an SQLite file for querying and modifying using the data commands.
    """)]
public class SqliteConnectionSource : ConnectionSource<SQLiteConnection, SQLiteCommand>
{
    [PrimarySetting("Data source filename")]
    private readonly UpdatingPrimaryValue DataSourceFileConstant = new();
    private string? CurrentDbPath;

    protected override SQLiteConnection CreateConnection() =>
        new(new SQLiteConnectionStringBuilder()
        {
            DataSource = this.CurrentDbPath, 
            JournalMode = SQLiteJournalModeEnum.Wal,
            Pooling = true,
            SyncMode = SynchronizationModes.Full,           
        }.ToString());
    protected override bool IsReloadRequired(StampedMap constants, IInteraction interaction)
    {
        if ((constants, DataSourceFileConstant).IsRereadRequired(out object? ds) &&
            ds?.ToString() is string goodPath &&
            goodPath.Trim() != CurrentDbPath?.Trim())
        {
            this.CurrentDbPath = goodPath;
            return true;
        }
        else
        {
            return false;
        }
    }
    protected override ICommandTextPreprocessor TextPreprocessor { get; } =
        new DefaultCommandTextPreprocessor("sqlite", '$');
}