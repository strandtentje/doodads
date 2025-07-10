namespace Ziewaar.RAD.Doodads.RKOP.Text;

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
        text.Text.Take(text.Position).Count(x => x == '\n') + 1;
    public static int GetCurrentCol(this CursorText text) =>
        text.Text.Take(text.Position).Reverse().TakeWhile(x => x != '\n').Count();
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
        string hint,
        out Token token)
    {
        var continued = text.TakeToken(description, out token);
        if (token.IsValid)
            return continued;
        else
        {
            Console.Write($"""
                Syntax error at {text.GetCurrentLine()}:{text.GetCurrentCol()}, 
                Expected: {description.HumanReadable}
                {hint}
                """);
            throw new SyntaxException(text, $"""
                Syntax error at {text.GetCurrentLine()}:{text.GetCurrentCol()}, 
                Expected: {description.HumanReadable}
                {hint}
                """);

        }

    }
}
