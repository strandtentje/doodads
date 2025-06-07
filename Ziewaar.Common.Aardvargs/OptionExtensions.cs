using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ziewaar.Common.Aardvargs;

public static class OptionExtensions
{
    public static T Get<T>(this SortedList<string, object> options, string key, T defaultValue = default!)
    {
        if (options.TryGetValue(key, out var value))
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }
}
