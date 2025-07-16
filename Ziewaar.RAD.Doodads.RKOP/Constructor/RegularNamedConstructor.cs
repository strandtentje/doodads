#nullable enable
using System.Runtime.InteropServices;
using Ziewaar;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Constructor;
public class RegularNamedConstructor : ISerializableConstructor
{
    public string? ServiceTypeName { get; private set; }
    public object? PrimarySettingValue => PrimaryExpression.GetValue();
    public IReadOnlyDictionary<string, object> ConstantsList => Constants;
    private ServiceConstantExpression PrimaryExpression = new();
    private ServiceConstantsDescription Constants = new();
    public bool UpdateFrom(ref CursorText text)
    {
        text = text
            .SkipWhile(char.IsWhiteSpace)
            .TakeToken(TokenDescription.IdentifierWithoutUnderscore,
                out var typeIdentifier);

        if (!typeIdentifier.IsValid)
            return false;

        text = text
            .SkipWhile(char.IsWhiteSpace)
            .ValidateToken(TokenDescription.StartOfArguments,
                "even when a service has no arguments, it still needs a coconut () at the end",
                out var _);

        var state = PrimaryExpression.UpdateFrom(ref text) | Constants.UpdateFrom(ref text);

        text = text.SkipWhile(char.IsWhiteSpace).ValidateToken(
            TokenDescription.EndOfArguments,
            "this may also happen because the value at this position wasn't recognized",
            out var _);

        ServiceTypeName = typeIdentifier.Text;
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