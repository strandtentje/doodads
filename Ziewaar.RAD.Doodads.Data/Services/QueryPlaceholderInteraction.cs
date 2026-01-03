using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Data.Services;

public class QueryPlaceholderInteraction(
    IInteraction interaction,
    object? constantsPrimaryConstant,
    IReadOnlyDictionary<string, object> constantsNamedItems) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    private readonly string PrimaryPlaceholder = constantsPrimaryConstant?.ToString() ?? "";
    private readonly IReadOnlyDictionary<string, string> NamedPlaceholders = constantsNamedItems
        .Where(x => !string.IsNullOrWhiteSpace(x.Value?.ToString())).ToDictionary(x => x.Key, x => x.Value.ToString(),
            StringComparer.OrdinalIgnoreCase);

    private const string PLACEHOLDER = nameof(PLACEHOLDER);

    public string Apply(string commandCommandText)
    {
        using var reader = new StringReader(commandCommandText);
        var output = new StringBuilder();
        while (reader.ReadLine() is { } line)
        {
            if (line.StartsWith(PLACEHOLDER))
            {
                var nameOfPlaceholder = line.Substring(PLACEHOLDER.Length).Trim();

                if (string.IsNullOrWhiteSpace(nameOfPlaceholder))
                    output.AppendLine(PrimaryPlaceholder);
                else if (NamedPlaceholders.TryGetValue(nameOfPlaceholder, out var replacement))
                    output.AppendLine(replacement);
            }
            else
            {
                output.AppendLine(line);
            }
        }

        return output.ToString();
    }
}