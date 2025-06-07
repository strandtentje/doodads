using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Ziewaar.RAD.Doodads.RKOP.Exceptions;


namespace Ziewaar.RAD.Doodads.RKOP;

public class ServiceConstantExpression : IParityParser
{
    public ConstantType ConstantType;
    private ServiceConstantsDescription Set;
    private ServiceConstantsMember Member;
    public string TextValue;
    public (string WorkingDirectory, string RelativePath) PathValue;
    public bool BoolValue;
    public decimal NumberValue;
    private ServiceConstantExpression[] ArrayItems = [];

    public object GetValue() => ConstantType switch
    {
        ConstantType.String => TextValue,
        ConstantType.Bool => BoolValue,
        ConstantType.Number => NumberValue,
        ConstantType.Path => PathValue,
        ConstantType.Reference => Member.Value.GetValue(),
        ConstantType.Array => ArrayItems.Select(x => x.GetValue()),
        _ => throw new InvalidOperationException(),
    };

    public ParityParsingState UpdateFrom(ref CursorText inText)
    {
        var text = inText.SkipWhile(char.IsWhiteSpace);

        text = text.TakeToken(TokenDescription.ArrayOpen, out var aop);
        if (aop.IsValid)
        {
            ParityParsingState arrRes = ConsumeRemainingArrayIncludingCloser(ref text);
            inText = text;
            return arrRes;
        }

        text = text.TakeToken(TokenDescription.RelativePathAnnouncement, out var rpa);
        if (rpa.IsValid)
        {
            var pathRes = ConsumeRemainingStringIncludingQuotes(ref text, x => SetPathValue(text.WorkingDirectory, x));
            inText = text;
            return pathRes;
        }

        text = text.TakeToken(TokenDescription.DoubleQuotes, out var dqt);
        if (dqt.IsValid)
        {
            var stringRes = ConsumeRemainingStringIncludingQuotes(ref text, SetStringValue);
            inText = text;
            return stringRes;
        }

        text = text.TakeToken(TokenDescription.Identifier, out var firstId);
        if (firstId.IsValid && bool.TryParse(firstId.Text, out bool resultBool))
        {
            inText = text;
            return SetBoolValue(resultBool);
        }
        else if (firstId.IsValid)
        {
            text = text.TakeToken(TokenDescription.DecimalSeparator, out var point);
            if (point.IsValid)
            {
                text = text.TakeToken(TokenDescription.Identifier, out var secondId);
                if (secondId.IsValid &&
                    text[$"const_{firstId.Text}"] is ServiceConstantsDescription scd &&
                    scd.Members.SingleOrDefault(x => x.Key == secondId.Text) is ServiceConstantsMember member)
                {
                    inText = text;
                    return this.SetReferencedValue(scd, member);
                }
                else
                {
                    throw new ReferenceException($"No constants set called {firstId.Text} in scope, or it has no member called {secondId.Text}");
                }
            }
            else
            {
                throw new ReferenceException($"Identifier as expression must always be true, false or Reference.SeparatedByPeriod");
            }
        }

        text = text.TakeToken(TokenDescription.Numbers, out var wholeNums);
        if (wholeNums.IsValid && int.TryParse(wholeNums.Text, out int res))
        {
            decimal candidate = res;
            text = text.TakeToken(TokenDescription.DecimalSeparator, out var dp);
            if (dp.IsValid)
            {
                text = text.TakeToken(TokenDescription.Numbers, out var decimals);
                if (decimals.IsValid &&
                    decimal.TryParse(
                        $"0.{decimals.Text}",
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture.NumberFormat,
                        out decimal decimalValue))
                {
                    inText = text;
                    return this.SetDecimalValue(candidate + decimalValue);
                }
                else
                {
                    throw new ParsingException($"Saw decimal point, but no remaining decimals");
                }
            }
            else
            {
                inText = text;
                return SetDecimalValue(candidate);
            }
        }
        else if (wholeNums.IsValid)
        {
            throw new ParsingException($"Unable to parse number.");
        }

        return ParityParsingState.Void;
    }

    private ParityParsingState ConsumeRemainingArrayIncludingCloser(ref CursorText text)
    {
        List<ServiceConstantExpression> newArrayExpressions = new();
        while (true)
        {
            text = text.SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.ArrayClose, out var closer);
            if (closer.IsValid)
                break;
            var newItem = new ServiceConstantExpression();
            newArrayExpressions.Add(newItem);
            newItem.UpdateFrom(ref text);
            Token comma;
            do
            {
                text = text.SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.ArgumentSeparator, out comma);
            } while (comma.IsValid);
        }
        ParityParsingState state = ParityParsingState.Unchanged;
        if (this.ConstantType != ConstantType.Array)
            state = ParityParsingState.Changed;
        else if (this.ArrayItems.Length != newArrayExpressions.Count)
            state = ParityParsingState.Changed;
        else if (this.ArrayItems.Zip(newArrayExpressions, (x, y) => (x, y)).Any(p => p.x.Mismatches(p.y)))
            state = ParityParsingState.Changed;
        this.ConstantType = ConstantType.Array;
        this.ArrayItems = newArrayExpressions.ToArray();
        return state;
    }

    private ParityParsingState SetPathValue(DirectoryInfo workingDirectory, string x)
    {
        var state = ParityParsingState.Unchanged;
        if (this.ConstantType != ConstantType.Path) state |= ParityParsingState.Changed;
        if (this.PathValue != (workingDirectory.FullName, x)) state |= ParityParsingState.Changed;
        this.ConstantType = ConstantType.Path;
        this.PathValue = (workingDirectory.FullName, x);

        return state;
    }

    private ParityParsingState SetDecimalValue(decimal v)
    {
        var state = ParityParsingState.Unchanged;
        if (this.ConstantType != ConstantType.Number) state |= ParityParsingState.Changed;
        if (this.NumberValue != v) state |= ParityParsingState.Changed;

        this.ConstantType = ConstantType.Number;
        this.NumberValue = v;

        return state;
    }

    private ParityParsingState SetReferencedValue(ServiceConstantsDescription desc, ServiceConstantsMember mem)
    {
        this.ConstantType = ConstantType.Reference;
        this.Set = desc;
        this.Member = mem;
        return ParityParsingState.Changed;
    }

    private ParityParsingState SetBoolValue(bool resultBool)
    {
        var state = ParityParsingState.Unchanged;
        if (this.ConstantType != ConstantType.Bool) state |= ParityParsingState.Changed;
        if (this.BoolValue != resultBool) state |= ParityParsingState.Changed;

        this.ConstantType = ConstantType.Bool;
        this.BoolValue = resultBool;

        return state;
    }

    private ParityParsingState ConsumeRemainingStringIncludingQuotes(
        ref CursorText inText, Func<string, ParityParsingState> process)
    {
        StringBuilder guts = new StringBuilder();
        var text = inText;
        while (true)
        {
            text = text.TakeToken(TokenDescription.StringGuts, out var stringSegment);
            guts.Append(stringSegment.Text);
            text = text.TakeToken(TokenDescription.EscapeSequence, out var slash);
            if (!slash.IsValid)
            {
                inText = text.ValidateToken(TokenDescription.DoubleQuotes, out var _);
                return process(guts.ToString());
            }
            else
            {
                guts.Append(slash.Text[1]);
            }
        }
    }

    private ParityParsingState SetStringValue(string v)
    {
        var state = ParityParsingState.Unchanged;
        if (this.ConstantType != ConstantType.String) state |= ParityParsingState.Changed;
        if (this.TextValue != v) state |= ParityParsingState.Changed;

        this.ConstantType = ConstantType.String;
        this.TextValue = v;

        return state;
    }

    internal bool Mismatches(ServiceConstantExpression value)
    {
        if (this.ConstantType != value.ConstantType) return true;

        switch (this.ConstantType)
        {
            case ConstantType.String:
                return this.TextValue != value.TextValue;
            case ConstantType.Bool:
                return this.BoolValue != value.BoolValue;
            case ConstantType.Number:
                return this.NumberValue != value.NumberValue;
            case ConstantType.Reference:
                return this.Set.BranchKey != value.Set.BranchKey || this.Member.Key != value.Member.Key || this.Member.Value.Mismatches(value.Member.Value);
            case ConstantType.Path:
                return this.PathValue != value.PathValue;
            case ConstantType.Array:
                if (this.ArrayItems.Length != value.ArrayItems.Length)
                    return true;
                return this.ArrayItems.Zip(value.ArrayItems, (x, y) => (x, y)).Any(p => p.x.Mismatches(p.y));
            default:
                return true;
        }
    }
    public void WriteTo(StreamWriter writer)
    {
        switch (this.ConstantType)
        {
            case ConstantType.String:
                writer.Write('"');
                writer.Write(this.TextValue.Replace(@"\", @"\\").Replace(@"""", @"\"""));
                writer.Write('"');
                break;
            case ConstantType.Bool:
                writer.Write(this.BoolValue ? "True" : "False");
                break;
            case ConstantType.Number:
                if (Math.Floor(this.NumberValue) == this.NumberValue)
                    writer.Write(((int)this.NumberValue).ToString());
                else
                    writer.Write(this.NumberValue.ToString(CultureInfo.InvariantCulture));
                break;
            case ConstantType.Reference:
                writer.Write(this.Set.BranchKey);
                writer.Write(".");
                writer.Write(this.Member.Key);
                break;
            case ConstantType.Path:
                writer.Write(@"f""");
                writer.Write(this.PathValue.RelativePath.Replace(@"\", @"\\").Replace(@"""", @"\"""));
                writer.Write(@"""");
                break;
            case ConstantType.Array:
                writer.Write("[");
                for (int i = 0; i < this.ArrayItems.Length; i++)
                {
                    this.ArrayItems[i].WriteTo(writer);
                    if (i + 1 < this.ArrayItems.Length)
                        writer.Write(", ");
                }
                writer.Write("]");
                break;
            default:
                break;
        }
    }
}
