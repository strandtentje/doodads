#nullable enable
using Ziewaar.RAD.Doodads.ModuleLoader.RkopLanguage.Text;

namespace Ziewaar.RAD.Doodads.ModuleLoader.RkopLanguage.Constructor.Shorthands;

public class RegularNamedConstructor : ISerializableConstructor
{
    public string? ServiceTypeName { get; set; }
    public object? PrimarySettingValue => PrimaryExpression.GetValue();
    public IReadOnlyDictionary<string, object> ConstantsList => Constants;
    public readonly ServiceConstantExpression PrimaryExpression = new();
    public readonly ServiceConstantsDescription Constants = new();
    public bool UpdateFrom(ref CursorText text)
    {
        text = text
            .SkipWhitespace()
            .TakeToken(TokenDescription.IdentifierWithoutUnderscore,
                out var typeIdentifier);

        if (!typeIdentifier.IsValid)
            return false;

        text = text
            .SkipWhitespace()
            .ValidateToken(TokenDescription.StartOfArguments,
                "even when a service has no arguments, it still needs a coconut () at the end",
                out var _);

        var state = PrimaryExpression.UpdateFrom(ref text) | Constants.UpdateFrom(ref text);

        var afterSingleArgBracket = text.SkipWhitespace().TakeToken(TokenDescription.EndOfArguments, out var cbToken);
        if (cbToken.IsValid)
        {
            text = afterSingleArgBracket;
            ServiceTypeName = typeIdentifier.Text;
            return true;
        }
        else
        {
            var arrex = new ServiceConstantExpression();
            arrex.UpdateFrom(ref text, true);
            var lst = new List<ServiceConstantExpression>();

            switch (PrimaryExpression.ConstantType)
            {
                case ConstantType.String:
                    lst.Add(new ServiceConstantExpression()
                    {
                        ConstantType = ConstantType.String,
                        TextValue = PrimaryExpression.TextValue,
                    });
                    break;
                case ConstantType.Bool:
                    lst.Add(new ServiceConstantExpression()
                    {
                        ConstantType = ConstantType.Bool,
                        BoolValue = PrimaryExpression.BoolValue,
                    });
                    break;
                case ConstantType.Number:
                    lst.Add(new ServiceConstantExpression()
                    {
                        ConstantType = ConstantType.Number,
                        NumberValue = PrimaryExpression.NumberValue,
                    });
                    break;
                case ConstantType.Path:
                    lst.Add(new ServiceConstantExpression()
                    {
                        ConstantType = ConstantType.Path,
                        PathValue = PrimaryExpression.PathValue,
                    });
                    break;
                case ConstantType.Array:
                    lst.AddRange(PrimaryExpression.ArrayItems);
                    break;
                default:
                    break;
            }

            lst.AddRange(arrex.ArrayItems);
            PrimaryExpression.ArrayItems = lst.ToArray();
            PrimaryExpression.ConstantType = ConstantType.Array;

            ServiceTypeName = typeIdentifier.Text;
        }


        text = text.SkipWhitespace().ValidateToken(
            TokenDescription.EndOfArguments,
            $"this may also happen because the value at this position wasn't recognized. At `{typeIdentifier.Text}` with `\"{PrimaryExpression.GetValue()?.ToString() ?? "{null}"}\"`",
            out var _);
        return true;

    }
    public void WriteTo(StreamWriter writer, int indentation)
    {
        writer.Write(ServiceTypeName);
        writer.Write('(');
        if (!string.IsNullOrWhiteSpace(PrimarySettingValue?.ToString()))
        {
            PrimaryExpression.WriteTo(writer);
            writer.Write(", ");
        }
        Constants.WriteTo(writer);
        writer.Write(')');
    }
}
public interface ISerializableConstructor
{
    string? ServiceTypeName { get; }
    object? PrimarySettingValue { get; }
    bool UpdateFrom(ref CursorText text);
    void WriteTo(StreamWriter writer, int indentation);
    IReadOnlyDictionary<string, object> ConstantsList { get; }
}