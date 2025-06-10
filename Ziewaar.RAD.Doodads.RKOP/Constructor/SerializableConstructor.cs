#nullable enable
using Ziewaar;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Constructor;
public class SerializableConstructor : IParityParser
{
    public string? ServiceTypeName { get; private set; }
    public ServiceConstantExpression PrimaryExpression { get; private set; } = new();
    public ServiceConstantsDescription Constants { get; private set; } = new();
    public ParityParsingState UpdateFrom(ref CursorText text)
    {
        text = text
            .SkipWhile(char.IsWhiteSpace)
            .TakeToken(TokenDescription.IdentifierWithoutUnderscore,
                out var typeIdentifier);

        if (!typeIdentifier.IsValid)
            return ParityParsingState.Void;

        text = text
            .SkipWhile(char.IsWhiteSpace)
            .ValidateToken(TokenDescription.StartOfArguments,
                out var _);        
        
        var state = PrimaryExpression.UpdateFrom(ref text) | Constants.UpdateFrom(ref text);

        text = text.SkipWhile(char.IsWhiteSpace).ValidateToken(TokenDescription.EndOfArguments, out var _);

        if (string.IsNullOrWhiteSpace(ServiceTypeName))
        {
            ServiceTypeName = typeIdentifier.Text;
            return state | ParityParsingState.New;
        }
        else if (ServiceTypeName != typeIdentifier.Text)
        {
            ServiceTypeName = typeIdentifier.Text;
            return state | ParityParsingState.Changed;
        }
        else
        {
            return state;
        }
    }
    public void WriteTo(StreamWriter writer, int indentation)
    {
        writer.Write(ServiceTypeName);
        writer.Write('(');
        Constants.WriteTo(writer);
        writer.Write(')');
    }
}