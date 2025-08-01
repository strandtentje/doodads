﻿using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Constructor;

public class ServiceConstantsMember : IParityParser
{
    public string Key;
    public ServiceConstantExpression Value = new();
    public ParityParsingState UpdateFrom(ref CursorText inText)
    {
        var text = inText.
            SkipWhile(char.IsWhiteSpace).
            TakeToken(TokenDescription.IdentifierWithoutUnderscore, out var identifier);

        if (!identifier.IsValid)
            return ParityParsingState.Void;

        text = text.
            SkipWhile(char.IsWhiteSpace).
            ValidateToken(
            TokenDescription.AssignmentOperator, 
            "this may also happen if the primary constant of this service couldn't be processed",
            out var _);

        var state = Value.UpdateFrom(ref text);

        if (state == ParityParsingState.Void)
            throw new ParsingException(text, "Expected value for assignment");

        if (string.IsNullOrWhiteSpace(Key))
            state |= ParityParsingState.New;
        else if (Key != identifier.Text)
            state |= ParityParsingState.Changed;
            Key = identifier.Text;

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
