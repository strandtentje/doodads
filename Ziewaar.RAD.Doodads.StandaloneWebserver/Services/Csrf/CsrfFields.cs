using System.Data.SQLite;
using Ziewaar.RAD.Doodads.Data;
using Ziewaar.RAD.Doodads.Data.Implementable;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Csrf;
#pragma warning disable 67
public class CsrfFields(
    CommandSourceInteraction<SQLiteCommand> commands,
    string instanceCookie,
    string csrfCookieValue) : ICsrfFields
{
    public string NewObfuscation(string formName, string deobfuscatedName)
    {
        var obfuscatedName = ComponentCookie.CreateNew().ToString();
        commands.UseCommand(command =>
        {
            command.CommandText =
                """
                INSERT INTO csrftoken (
                    instancetoken,
                    csrfanchorcookie,
                    formname,
                    fieldalias,
                    truefieldname,
                    inserttime
                ) VALUES (
                    $instancetoken,
                    $csrfanchorcookie,
                    $formname,
                    $fieldalias,
                    $truefieldname,
                    CURRENT_TIMESTAMP
                );                  
                """;
            command.SetParams(
                DbType.String.ToParam("instancetoken", instanceCookie),
                DbType.String.ToParam("csrfanchorcookie", csrfCookieValue),
                DbType.String.ToParam("formname", formName),
                DbType.String.ToParam("fieldalias", obfuscatedName),
                DbType.String.ToParam("truefieldname", deobfuscatedName));
            command.ExecuteNonQuery();
            return default(object);
        });
        return obfuscatedName;
    }
    public void UnregisterAlias(string formName, string alias)
    {
        commands.UseCommand(command =>
        {
            command.CommandText = """
            DELETE FROM csrftoken 
            WHERE instancetoken = $instancetoken 
            AND csrfanchorcookie = $csrfanchorcookie 
            AND formname = $formname
            AND fieldalias = $alias
            """;
            command.SetParams(
                DbType.String.ToParam("instancetoken", instanceCookie),
                DbType.String.ToParam("csrfanchorcookie", csrfCookieValue),
                DbType.String.ToParam("formname", formName),
                DbType.String.ToParam("alias", alias));
            command.ExecuteNonQuery();
            return default(object);
        });
    }
    public bool TryObfuscating(string formName, string unobfuscatedName, [NotNullWhen(true)] out string obfuscatedName)
    {
        var fieldAliases = commands.UseCommand(command =>
        {
            command.CommandText =
                """
                SELECT fieldalias
                FROM csrftoken
                WHERE inserttime >= datetime('now', '-30 minutes') 
                AND instancetoken = $instancetoken
                AND formname = $formname
                AND csrfanchorncookie = $csrfanchorcookie
                AND truefieldname = $truefieldname
                """;
            command.SetParams(
                DbType.String.ToParam("instancetoken", instanceCookie),
                DbType.String.ToParam("formname", formName),
                DbType.String.ToParam("csrfanchorcookie", csrfCookieValue),
                DbType.String.ToParam("truefieldname", unobfuscatedName));
            List<string> results = new();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    results.Add(reader.GetString(0));
            }
            return results.ToArray();
        });
        if (fieldAliases.Length == 1)
        {
            obfuscatedName = fieldAliases[0];
            return true;
        }
        else
        {
            obfuscatedName = "";
            return false;
        }
    }
    public bool TryDeobfuscating(string formName, string obfuscatedName, [NotNullWhen(true)] out string deobfuscatedName)
    {
        var trueFieldNames = commands.UseCommand(command =>
        {
            command.CommandText =
                """
                SELECT truefieldname
                FROM csrftoken
                WHERE inserttime >= datetime('now', '-30 minutes') 
                AND instancetoken = $instancetoken
                AND csrfanchorncookie = $csrfanchorcookie
                AND formname = $formname
                AND fieldalias = $fieldalias
                """;
            command.SetParams(
                DbType.String.ToParam("instancetoken", instanceCookie),
                DbType.String.ToParam("csrfanchorcookie", csrfCookieValue),
                DbType.String.ToParam("formname", formName),
                DbType.String.ToParam("fieldalias", obfuscatedName));
            List<string> results = new();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    results.Add(reader.GetString(0));
            }
            return results.ToArray();
        });
        if (trueFieldNames.Length == 1)
        {
            deobfuscatedName = trueFieldNames[0];
            return true;
        }
        else
        {
            deobfuscatedName = "";
            return false;
        }
    }

    public string[] GetSortedObfuscatedWhitelist(string formName)
    {
        return commands.UseCommand(command =>
        {
            command.CommandText =
                """
                SELECT fieldalias
                FROM csrftoken
                WHERE inserttime >= datetime('now', '-30 minutes') 
                AND instancetoken = $instancetoken
                AND csrfanchorncookie = $csrfanchorcookie
                AND formname = $formname
                ORDER BY inserttime
                """;
            command.SetParams(
                DbType.String.ToParam("instancetoken", instanceCookie),
                DbType.String.ToParam("csrfanchorcookie", csrfCookieValue),
                DbType.String.ToParam("formname", formName));
            List<string> results = new();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    results.Add(reader.GetString(0));
            }
            return results.ToArray();
        });
    }

    public string[] GetSortedDeobfuscatedWhitelist(string formName)
    {
        return commands.UseCommand(command =>
        {
            command.CommandText =
                """
                SELECT truefieldname
                FROM csrftoken
                WHERE inserttime >= datetime('now', '-30 minutes') 
                AND instancetoken = $instancetoken
                AND csrfanchorncookie = $csrfanchorcookie
                AND formname = $formname
                ORDER BY inserttime
                """;
            command.SetParams(
                DbType.String.ToParam("instancetoken", instanceCookie),
                DbType.String.ToParam("csrfanchorcookie", csrfCookieValue),
                DbType.String.ToParam("formname", formName));
            List<string> results = new();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    results.Add(reader.GetString(0));
            }
            return results.ToArray();
        });
    }
}