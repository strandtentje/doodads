using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Data.FlatFile.Support;

namespace Ziewaar.RAD.Doodads.Data.FlatFile;

[Category("Databases & Querying")]
[Title("Query a flatfile's properties")]
[Description("""
    Provided an open flatfile, iterate thru members that have a certain assignment. 
    Provide the name of the property in the primary constant. The value will be taken from register.
    Provide the desired members as named constants with their defaults.
    """)]
public class FlatFileKeysWith : IteratingService
{
    [PrimarySetting("Provide name of property to look for")]
    private readonly UpdatingPrimaryValue NameOfProperty = new UpdatingPrimaryValue();
    protected override bool RunElse { get; } = false;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        string forKey;
        ComparisonType comparison = ComparisonType.Equals;
        decimal searchNum = 0;
        bool numericCompare = TryReduceToNumber(repeater.Register, out searchNum);
        string searchString = ReduceToString(repeater.Register);

        if (constants.NamedItems.TryGetValue("neq", out var neqObj) && neqObj.ToString() is { } notEqualsCandidate)
        {
            forKey = notEqualsCandidate;
            comparison = ComparisonType.NotEquals;
        }
        else if (constants.NamedItems.TryGetValue("leq", out var leqObj) && leqObj.ToString() is { } lessEqualsCandidate)
        {
            forKey = lessEqualsCandidate;
            comparison = ComparisonType.LesserInclusive;
        }
        else if (constants.NamedItems.TryGetValue("geq", out var geqObj) && geqObj.ToString() is { } greaterEqualsCandidate)
        {
            forKey = greaterEqualsCandidate;
            comparison = ComparisonType.GreaterInclusive;
        }
        else if (constants.NamedItems.TryGetValue("lt", out var ltObj) && ltObj.ToString() is { } lessThanCandidate)
        {
            forKey = lessThanCandidate;
            comparison = ComparisonType.LesserInclusive;
        }
        else if (constants.NamedItems.TryGetValue("gt", out var gtObj) && gtObj.ToString() is { } greaterThanCandidate)
        {
            forKey = greaterThanCandidate;
            comparison = ComparisonType.GreaterInclusive;
        }
        else if (constants.PrimaryConstant.ToString() is string keyCandidate)
        {
            forKey = keyCandidate;
            comparison = ComparisonType.Equals;
        }
        else
        {
            throw new BasicException("expected search key in primary constant");
        }

        if (!repeater.TryGetCustom<JsonFile>(out var flatFile) || flatFile == null)
            throw new BasicException("no flat file in scope");

        var items = flatFile.ListAll(forKey, storedValue =>
        {
            if (!numericCompare)
            {
                var castValue = ReduceToString(storedValue);
                return comparison switch
                {
                    ComparisonType.Equals => StringComparer.Ordinal.Equals(searchString, castValue),
                    ComparisonType.NotEquals => !StringComparer.Ordinal.Equals(searchString, castValue),
                    ComparisonType.Greater => StringComparer.Ordinal.Compare(castValue, searchString) > 0,
                    ComparisonType.Lesser => StringComparer.Ordinal.Compare(castValue, searchString) < 0,
                    ComparisonType.GreaterInclusive => StringComparer.Ordinal.Compare(castValue, searchString) >= 0,
                    ComparisonType.LesserInclusive => StringComparer.Ordinal.Compare(castValue, searchString) <= 0,
                    _ => false,
                };
            }

            if (TryReduceToNumber(storedValue, out var val))
                return comparison switch
                {
                    ComparisonType.Equals => val == searchNum,
                    ComparisonType.NotEquals => val != searchNum,
                    ComparisonType.Greater => val > searchNum,
                    ComparisonType.Lesser => val < searchNum,
                    ComparisonType.GreaterInclusive => val >= searchNum,
                    ComparisonType.LesserInclusive => val <= searchNum,
                    _ => false,
                };

            return false;
        });

        foreach (var item in items)
            yield return repeater.AppendRegister(item.Key).AppendMemory(new FlatFileMemberMemory(constants.NamedItems, item.Value));
    }

    public static bool TryReduceToNumber(object? value, out decimal dec)
    {
        dec = 0M;
        if (value == null) return false;
        try
        {
            dec = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
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

public enum ComparisonType { Equals, NotEquals, Greater, Lesser, GreaterInclusive, LesserInclusive, }