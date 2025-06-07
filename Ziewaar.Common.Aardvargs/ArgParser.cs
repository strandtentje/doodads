using System.Globalization;
using System.Linq;

namespace Ziewaar.Common.Aardvargs;

public static class ArgParser
{
    public static ParsedArgs Parse(string[] args)
    {
        var result = new ParsedArgs();
        bool stopParsingOptions = false;

        for (int i = 0; i < args.Length; i++)
        {
            string rawArg = args[i];
            string arg = StripQuotes(rawArg);

            if (arg == "--")
            {
                stopParsingOptions = true;
                continue;
            }

            if (!stopParsingOptions && arg.StartsWith("--"))
            {
                var parts = arg.Substring(2).Split(['='], 2);
                string key = parts[0];
                object value;
                if (parts.Length == 2)
                {
                    value = Coerce(parts[1]);
                }
                else
                {
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        value = Coerce(StripQuotes(args[++i]));
                    }
                    else
                    {
                        value = true;
                    }
                }

                result.Options[key] = value;
            }
            else if (!stopParsingOptions && arg.StartsWith("-") && arg.Length > 1)
            {
                string flags = arg.Substring(1);

                if (flags.Contains('='))
                {
                    var parts = flags.Split(['='], 2);
                    result.Options[parts[0]] = Coerce(parts[1]);
                }
                else if (flags.Length > 1 && (i + 1 >= args.Length || args[i + 1].StartsWith("-")))
                {
                    foreach (char flag in flags)
                        result.Options[flag.ToString()] = true;
                }
                else
                {
                    string key = flags;
                    object value = (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                        ? Coerce(StripQuotes(args[++i]))
                        : true;
                    result.Options[key] = value;
                }
            }
            else
            {
                result.Filenames.Add(arg);
            }
        }

        return result;
    }

    private static string StripQuotes(string s)
    {
        return s.StartsWith("\"") && s.EndsWith("\"") && s.Length >= 2
            ? s.Substring(1).Substring(0, s.Length-2)
            : s;
    }

    private static object Coerce(string s)
    {
        if (bool.TryParse(s, out var b)) return b;
        if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var i)) return i;
        if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
        return s;
    }
}