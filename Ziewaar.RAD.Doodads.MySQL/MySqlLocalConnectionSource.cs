using MySqlConnector;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.SQLite;

namespace Ziewaar.RAD.Doodads.MySQL;
[Category("Data")]
[Title("Connect to MySQL")]
[Description("""
    Connects to mysql at localhost, with the username, password and database parameters having the same name
    in the connection string. Usually "good enough" for local use but be aware of the implications.
    """)]
public class MySqlLocalConnectionSource : ConnectionSource<MySqlConnection, MySqlCommand>
{
    [PrimarySetting("Databasename, username and password")]
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