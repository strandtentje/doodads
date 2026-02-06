#nullable enable
using System.Collections.ObjectModel;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Constructor.Shorthands;

public class CapturedShorthandConstructor : ISerializableConstructor
{
    public enum ShorthandType
    {
        NoShorthand,
        InvalidShorthand,
        Definition,
        Call,
        CallWildcard,
        ReturnThen,
        ReturnElse,
        Continue
    };
    public ShorthandType CurrentShorthandType { get; private set; } = ShorthandType.InvalidShorthand;
    public string? ServiceTypeName
    {
        get => CurrentShorthandType switch
        {
            ShorthandType.InvalidShorthand => throw new Exception("Unknown shorthand"),
            ShorthandType.CallWildcard => "Call",
            _ => Enum.GetName(typeof(ShorthandType), CurrentShorthandType)
        };
        set
        {
            CurrentShorthandType = (ShorthandType)Enum.Parse(typeof(ShorthandType), value ?? "InvalidShorthand");
            if (CurrentShorthandType is ShorthandType.InvalidShorthand or ShorthandType.NoShorthand)
                throw new Exception("May only set this to Definition, Call or Case");
        }
    }
    public object PrimarySettingValue => PrimaryExpression.GetValue();
    public IReadOnlyDictionary<string, object> ConstantsList => Constants;
    private ServiceConstantExpression PrimaryExpression = new();
    private ServiceConstantsDescription Constants = new();
    public bool UpdateFrom(ref CursorText text)
    {
        var temporaryCursorPosition = text
            .SkipWhitespace()
            .TakeToken(TokenDescription.BeakOpen, out var firstBeak)
            .TakeToken(TokenDescription.AsteriskInfix, out var asteriskInfix)
            .TakeToken(TokenDescription.BeakOpen, out var secondBeak)
            .TakeToken(TokenDescription.ArrayOpen, out var firstBlock)
            .TakeToken(TokenDescription.DefaultBranchCoupler, out var colonAfterBlock)
            .TakeToken(TokenDescription.Pipe, out var pipeAfterBlock);

        CurrentShorthandType = (
            firstBeak.IsValid, asteriskInfix.IsValid, secondBeak.IsValid,
            firstBlock.IsValid, colonAfterBlock.IsValid, pipeAfterBlock.IsValid) switch
        {
            (false, false, false, false, false, false) => ShorthandType.NoShorthand,
            (true, false, false, false, false, false) => ShorthandType.Call,
            (true, true, false, false, false, false) => ShorthandType.CallWildcard,
            (true, false, true, false, false, false) => ShorthandType.Definition,
            (false, false, false, true, false, false) => ShorthandType.Continue,
            (false, false, false, true, true, false) => ShorthandType.ReturnThen,
            (false, false, false, true, false, true) => ShorthandType.ReturnElse,
            _ => ShorthandType.InvalidShorthand,
        };

        if (CurrentShorthandType == ShorthandType.NoShorthand) return false;
        if (CurrentShorthandType == ShorthandType.InvalidShorthand)
            throw new SyntaxException(text, "Strange shorthand configuration; likely to do with <'s and ['s.");

        text = temporaryCursorPosition;

        if (CurrentShorthandType != ShorthandType.CallWildcard)
        {
            var state = PrimaryExpression.UpdateFrom(ref text) | Constants.UpdateFrom(ref text);
        }
        else
        {
            PrimaryExpression.ConstantType = ConstantType.String;
            PrimaryExpression.TextValue = "*";
        }

        switch (CurrentShorthandType)
        {
            case ShorthandType.ReturnThen:
            case ShorthandType.ReturnElse:
            case ShorthandType.Continue:
                text = text.SkipWhitespace().ValidateToken(TokenDescription.ArrayClose,
                    "likely forgot to match case close with ]", out var _);
                break;
            case ShorthandType.Definition:
                text = text.SkipWhitespace()
                    .ValidateToken(TokenDescription.BeakClose, "close definition with double >>", out var _)
                    .ValidateToken(TokenDescription.BeakClose, "close definition with double >>", out var _);
                break;
            case ShorthandType.Call:
            case ShorthandType.CallWildcard:
                text = text.SkipWhitespace()
                    .ValidateToken(TokenDescription.BeakClose, "close definition with single >", out var _);
                break;
            default:
                throw new SyntaxException(text, "don't know which shorthand we're closing.");
        }

        return true;
    }
    public void WriteTo(StreamWriter writer, int indentation)
    {
        switch (CurrentShorthandType)
        {
            case ShorthandType.ReturnElse:
                writer.Write("[|");
                PrimaryExpression.WriteTo(writer);
                Constants.WriteTo(writer);
                writer.Write(']');
                break;
            case ShorthandType.ReturnThen:
                writer.Write("[:");
                PrimaryExpression.WriteTo(writer);
                Constants.WriteTo(writer);
                writer.Write(']');
                break;
            case ShorthandType.Definition:
                writer.Write("<<");
                PrimaryExpression.WriteTo(writer);
                Constants.WriteTo(writer);
                writer.Write(">>");
                break;
            case ShorthandType.Call:
                writer.Write("<");
                PrimaryExpression.WriteTo(writer);
                Constants.WriteTo(writer);
                writer.Write(">");
                break;
        }
    }
}