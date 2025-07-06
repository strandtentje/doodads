using Microsoft.Data.Sqlite;
using Ziewaar.RAD.Doodads.Data;
using Ziewaar.RAD.Doodads.Data.Implementable;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Csrf;
#pragma warning disable 67
public class CsrfFields(
    CommandSourceInteraction<SqliteCommand> commands,
    string instanceCookie,
    string csrfCookieValue) : ICsrfFields
{
    public string PackField(string formName, string fieldName)
    {
        var fieldAlias = ComponentCookie.CreateNew().ToString();
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
                DbType.String.ToParam("fieldalias", fieldAlias),
                DbType.String.ToParam("truefieldname", fieldName));
            return default(object);
        });
        return fieldAlias;
    }
    public bool TryRecoverByTrueName(string formName, string trueName, [NotNullWhen(true)] out string fieldAlias)
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
                DbType.String.ToParam("truefieldname", trueName));
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
            fieldAlias = fieldAliases[0];
            return true;
        }
        else
        {
            fieldAlias = "";
            return false;
        }
    }
    public bool TryRecoverByAlias(string formName, string fieldAlias, [NotNullWhen(true)] out string trueFieldName)
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
                DbType.String.ToParam("fieldalias", fieldAlias));
            List<string> results = new();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    results.Add(reader.GetString(0));
            }
            return results.ToArray();
        });
        commands.UseCommand(command =>
        {
            command.CommandText = "DELETE FROM csrftoken WHERE fieldalias = $fieldalias";
            command.SetParams(DbType.String.ToParam("fieldalias", fieldAlias));
            command.ExecuteNonQuery();
            return default(object);
        });
        if (trueFieldNames.Length == 1)
        {
            trueFieldName = trueFieldNames[0];
            return true;
        }
        else
        {
            trueFieldName = "";
            return false;
        }
    }
    
    public string[] GetWhitelist(string formName)
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