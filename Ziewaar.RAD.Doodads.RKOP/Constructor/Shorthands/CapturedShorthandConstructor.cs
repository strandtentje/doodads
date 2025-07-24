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
        ReturnThen,
        ReturnElse,
    };

    public ShorthandType CurrentShorthandType { get; private set; } = ShorthandType.InvalidShorthand;

    public string? ServiceTypeName
    {
        get =>
            CurrentShorthandType != ShorthandType.InvalidShorthand
                ? Enum.GetName(typeof(ShorthandType), CurrentShorthandType)
                : throw new Exception("Unknown Shorthand");
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
            .SkipWhile(char.IsWhiteSpace)
            .TakeToken(TokenDescription.BeakOpen, out var firstBeak)
            .TakeToken(TokenDescription.BeakOpen, out var secondBeak)
            .TakeToken(TokenDescription.ArrayOpen, out var firstBlock)
            .TakeToken(TokenDescription.DefaultBranchCoupler, out var colonAfterBlock)
            .TakeToken(TokenDescription.Pipe, out var pipeAfterBlock);

        CurrentShorthandType = (firstBeak.IsValid, secondBeak.IsValid, firstBlock.IsValid, colonAfterBlock.IsValid,
                pipeAfterBlock.IsValid) switch
            {
                (false, false, false, false, false) => ShorthandType.NoShorthand,
                (true, false, false, false, false) => ShorthandType.Call,
                (true, true, false, false, false) => ShorthandType.Definition,
                (false, false, true, true, false) => ShorthandType.ReturnThen,
                (false, false, true, false, true) => ShorthandType.ReturnElse,
                _ => ShorthandType.InvalidShorthand,
            };

        if (CurrentShorthandType == ShorthandType.NoShorthand) return false;
        if (CurrentShorthandType == ShorthandType.InvalidShorthand)
            throw new SyntaxException(text, "Strange shorthand configuration; likely to do with <'s and ['s.");

        text = temporaryCursorPosition;

        var state = PrimaryExpression.UpdateFrom(ref text) | Constants.UpdateFrom(ref text);

        switch (CurrentShorthandType)
        {
            case ShorthandType.ReturnThen:
            case ShorthandType.ReturnElse:
                text = text.SkipWhile(char.IsWhiteSpace).ValidateToken(TokenDescription.ArrayClose,
                    "likely forgot to match case close with ]", out var _);
                break;
            case ShorthandType.Definition:
                text = text.SkipWhile(char.IsWhiteSpace)
                    .ValidateToken(TokenDescription.BeakClose, "close definition with double >>", out var _)
                    .ValidateToken(TokenDescription.BeakClose, "close definition with double >>", out var _);
                break;
            case ShorthandType.Call:
                text = text.SkipWhile(char.IsWhiteSpace)
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