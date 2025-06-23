using MySqlConnector;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.SQLite;

namespace Ziewaar.RAD.Doodads.MySQL;
public class MySqlLocalConnectionSource : ConnectionSource<MySqlConnection, MySqlCommand>
{
    private readonly UpdatingPrimaryValue LocalCredentialConst = new();
    private string? LocalCredential;
    private string? ConnectionString;
    protected override MySqlConnection CreateConnection() => new(ConnectionString);
    protected override bool IsReloadRequired(StampedMap constants, IInteraction interaction)
    {
        if ((constants, LocalCredentialConst).IsRereadRequired(out string? _localCredential) &&
            this.LocalCredential != _localCredential)
        {
            this.LocalCredential = _localCredential;
            var bld = new MySqlConnectionStringBuilder
            {
                AllowUserVariables = true,
                ConnectionLifeTime = 0,
                Password = LocalCredential,
                Pooling = true,
                Port = 3306,
                Database = LocalCredential,
                Server = "localhost",
                UserID = LocalCredential
            };
            this.ConnectionString = bld.ToString();
            return true;
        }
        else
        {
            return false;
        }
    }
    protected override ICommandTextPreprocessor TextPreprocessor { get; } =
        new DefaultCommandTextPreprocessor("mysql", '@');
}