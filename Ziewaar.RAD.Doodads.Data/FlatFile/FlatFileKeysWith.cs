using Define.Doodads.Expo.Timeline;
using System.Collections.Generic;
using System.Globalization;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Data.FlatFile.Support;

namespace Ziewaar.RAD.Doodads.Data.FlatFile;

public class FlatFileKeysWith : IteratingService
{
    protected override bool RunElse { get; } = false;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (constants.PrimaryConstant.ToString() is not string forKey)
            throw new BasicException("expected search key in primary constant");
        if (!repeater.TryGetCustom<JsonFile>(out var flatFile) || flatFile == null)
            throw new BasicException("no flat file in scope");
        var searchValue = ReduceToString(repeater.Register);

        var items = flatFile.ListAll(forKey, storedValue =>
        {
            var castValue = ReduceToString(storedValue);
            return castValue == searchValue;
        });

        foreach (var item in items)
            yield return repeater.AppendRegister(item.Key).AppendMemory(new FlatFileMemberMemory(constants.NamedItems, item.Value));
    }

    public static string ReduceToString(object? value)
    {
        switch (value)
        {
            case string str:
                return str;
            case decimal dec:
                return dec.ToString(CultureInfo.InvariantCulture);
            case float flt:
                return flt.ToString(CultureInfo.InvariantCulture);
            case double dbl:
                return dbl.ToString(CultureInfo.InvariantCulture);
            case int i:
                return i.ToString(CultureInfo.InvariantCulture);
            case long l:
                return l.ToString(CultureInfo.InvariantCulture);
            case short s:
                return s.ToString(CultureInfo.InvariantCulture);
            case byte b:
                return b.ToString(CultureInfo.InvariantCulture);
            case uint ui:
                return ui.ToString(CultureInfo.InvariantCulture);
            case ulong ul:
                return ul.ToString(CultureInfo.InvariantCulture);
            case ushort us:
                return us.ToString(CultureInfo.InvariantCulture);
            case sbyte sb:
                return sb.ToString(CultureInfo.InvariantCulture);
            case char c:
                return c.ToString();
            default:
                return "";
        }
    }
}
