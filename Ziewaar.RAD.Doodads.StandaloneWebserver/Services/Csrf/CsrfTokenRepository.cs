using Microsoft.Data.Sqlite;
using Ziewaar.RAD.Doodads.SQLite;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public class CsrfTokenRepository(
    string instanceCookie,
    SqliteConnectionSource connections,
    CommandSourceInteraction<SqliteCommand> commands) : IDisposable
{
    public static readonly CsrfTokenRepository Instance = CsrfTokenRepository.Create();
    private static CsrfTokenRepository Create()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var csrfStoreDb = Path.Join(appdata, "doodads", "csrf.sqlite");
        CommandSourceInteraction<SqliteCommand>? commands = null;
        var connectionSource = new SqliteConnectionSource();
        connectionSource.OnThen += (sender, interaction) =>
        {
            commands = (CommandSourceInteraction<SqliteCommand>)interaction;
        };
        connectionSource.Enter(new StampedMap(csrfStoreDb), StopperInteraction.Instance);
        commands!.UseCommand(cmd =>
        {
            cmd.CommandText =
                """
                CREATE TABLE IF NOT EXISTS csrftoken (
                    instancetoken TEXT,
                    csrfanchorcookie TEXT,
                    formname TEXT,
                    fieldalias TEXT,
                    truefieldname TEXT,
                    inserttime DATETIME DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY(instancetoken, csrfanchorcookie, formname, fieldalias)
                );

                CREATE INDEX IF NOT EXISTS idx_csrftoken_instancetoken ON csrftoken(instancetoken);
                CREATE INDEX IF NOT EXISTS idx_csrftoken_csrfanchorcookie ON csrftoken(csrfanchorcookie);
                CREATE INDEX IF NOT EXISTS idx_csrftoken_fieldalias ON csrftoken(fieldalias);
                CREATE INDEX IF NOT EXISTS idx_csrftoken_truefieldname ON csrftoken(truefieldname);
                CREATE INDEX IF NOT EXISTS idx_csrftoken_inserttime ON csrftoken(inserttime);
                CREATE INDEX IF NOT EXISTS idx_csrftoken_formname ON csrftoken(formname);
                """;
            cmd.ExecuteNonQuery();
            return default(object);
        });
        return new(ComponentCookie.CreateNew().ToString(), connectionSource, commands!);
    }
    public CsrfFields RecoverForCookie(string csrfCookieValue)
    {
        return new CsrfFields(
            commands,
            instanceCookie,
            csrfCookieValue);
    }
    public void Dispose()
    {
        commands.UseCommand(cmd =>
        {
            cmd.CommandText =
                """
                DELETE FROM csrftoken
                WHERE instancetoken = $instancetoken;
                """;
            var param = cmd.CreateParameter();
            param.DbType = DbType.String;
            param.ParameterName = "instancetoken";
            param.Value = instanceCookie.ToString();
            cmd.Parameters.Add(param);
            cmd.ExecuteNonQuery();
            return default(object);
        });
        connections.Dispose();
    }
}