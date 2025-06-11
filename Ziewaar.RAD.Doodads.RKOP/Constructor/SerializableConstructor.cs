#nullable enable
using Ziewaar;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Constructor;
public class SerializableConstructor
{
    public string? ServiceTypeName { get; private set; }
    public ServiceConstantExpression PrimaryExpression { get; private set; } = new();
    public ServiceConstantsDescription Constants { get; private set; } = new();
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
        Constants.WriteTo(writer);
        writer.Write(')');
    }
}