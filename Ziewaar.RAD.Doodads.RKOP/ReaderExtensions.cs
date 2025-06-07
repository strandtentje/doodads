using System;
using System.Text;
using Ziewaar.RAD.Doodads.RKOP.Exceptions;


namespace Ziewaar.RAD.Doodads.RKOP;

public static class ReaderExtensions
{
    public static CursorText SkipWhile(this CursorText text, Func<char, bool> predicate)
    {
        int position = text.Position;
        while (position < text.Text.Length && predicate(text.Text[position]))
            position++;
        return text.AdvanceTo(position);
    }

    public static int GetCurrentLine(this CursorText text) =>
        Math.Abs(Array.BinarySearch(text.LinePositions, text.Position));
    public static int GetCurrentCol(this CursorText text) =>
        text.Position - text.LinePositions[text.GetCurrentLine() - 1];
    public static CursorText TakeToken(
        this CursorText text,
        TokenDescription description,
        out Token token)
    {
        int startingPosition = text.Position;
        StringBuilder resultBuilder = new StringBuilder();
        int nextPosition = startingPosition;
        for (; nextPosition < text.Text.Length && description.Predicate(nextPosition - startingPosition, text.Text[nextPosition]); nextPosition++)
            resultBuilder.Append(text.Text[nextPosition]);
        var resultString = resultBuilder.ToString();
        token = new(description, resultString, description.Validate(resultString));
        if (token.IsValid)
            return text.AdvanceTo(nextPosition);
        else
            return text;
    }

    public static CursorText ValidateToken(
        this CursorText text,
        TokenDescription description,
        out Token token)
    {
        var continued = text.TakeToken(description, out token);
        if (token.IsValid)
            return continued;
        else
            throw new SyntaxException($"""
                Syntax error at {text.GetCurrentLine()}:{text.GetCurrentCol()}, 
                Expected: {description.HumanReadable}
                """);

    }
}
