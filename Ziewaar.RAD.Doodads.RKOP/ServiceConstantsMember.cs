namespace Ziewaar.RAD.Doodads.RKOP;

public class ServiceConstantsMember : IParityParser
{
    public string Key;
    public ServiceConstantExpression Value = new();
    public ParityParsingState UpdateFrom(ref CursorText inText)
    {
        var text = inText.
            SkipWhile(char.IsWhiteSpace).
            TakeToken(TokenDescription.Identifier, out var identifier);

        if (!identifier.IsValid)
            return ParityParsingState.Void;

        text = text.
            SkipWhile(char.IsWhiteSpace).
            ValidateToken(TokenDescription.AssignmentOperator, out var _);

        var state = Value.UpdateFrom(ref text);

        if (state == ParityParsingState.Void)
            throw new ParsingException("Expected value for assignment");

        if (string.IsNullOrWhiteSpace(this.Key))
            state |= ParityParsingState.New;
        else if (this.Key != identifier.Text)
            state |= ParityParsingState.Changed;
            this.Key = identifier.Text;

        inText = text;
        return state;            
    }
    public void WriteTo(StreamWriter writer)
    {
        writer.Write(Key);
        writer.Write(" = ");
        Value.WriteTo(writer);
    }
}
