using System.Runtime.CompilerServices;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Doodads.RKOP.Text;

public static class ReaderExtensions
{
    public static CursorText SkipWhitespace(this CursorText text) => text.SkipWhile(char.IsWhiteSpace);
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

    private static Dictionary<TokenDescription, string> TokenNameLUT =
        typeof(TokenDescription).
        GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).
        Where(x => typeof(TokenDescription).IsAssignableFrom(x.FieldType)).
        ToDictionary(x => (TokenDescription)x.GetValue(null), x => x.Name);

    public static CursorText TakeToken(
        this CursorText text,
        TokenDescription description,
        out Token token,
        [CallerFilePath] string origin = "",
        [CallerLineNumber] int position = 0,
        [CallerMemberName] string member = "")
    {
        int startingPosition = text.Position;
        StringBuilder resultBuilder = new StringBuilder();
        int nextPosition = startingPosition;
        for (; nextPosition < text.Text.Length && description.Predicate(nextPosition - startingPosition, text.Text[nextPosition]); nextPosition++)
            resultBuilder.Append(text.Text[nextPosition]);
        var resultString = resultBuilder.ToString();
        token = new(description, resultString, description.Validate(resultString));

        text.LogParsing(nameof(TakeToken), origin, position, member, token);

        if (token.IsValid)
        {
            if (description == TokenDescription.BlockOpen)
                text.Stack.Add(text.LastIdentifier);
            else if (description == TokenDescription.BlockClose && text.Stack.Count > 0)
            {
                text.LastIdentifier = text.Stack.Last();
                text.Stack.RemoveAt(text.Stack.Count - 1);
            }
            else if (description == TokenDescription.BlockClose && text.Stack.Count == 0)
                text.LastIdentifier = "Root";
            else if (description == TokenDescription.Identifier ||
                description == TokenDescription.IdentifierWithoutUnderscore)
                text.LastIdentifier = token.Text;

            return text.AdvanceTo(nextPosition);
        }
        else
        {

            return text;
        }
    }

    public static void LogParsing(
        this CursorText text, string operation, string caller, int line, string function, Token token)
    {
        if (!text.LogFileExists)
            return;

        var rkopFile = Path.GetFileNameWithoutExtension(text.BareFile);
        var csFile = Path.GetFileNameWithoutExtension(caller);

        var accos = string.Join(" { ", text.Stack);
        var rkopDesc = $"{rkopFile}:{text.GetCurrentLine()}:{text.GetCurrentCol()}";
        var csDesc = $"{csFile}:{function}@{line}";
        var parseDesc = $"{operation}/{(TokenNameLUT.TryGetValue(token.Description, out string tok) ? tok : token.Description.HumanReadable)}";
        var outDesc = $"{token.Text} ({(token.IsValid ? "VALID" : "INVALID")})";

        text.EnqueueLogMessage($"""
            {new string('=', 100)}
            {new string(' ', 99 * text.Position / text.Text.Length)}^
            [{text.Depth}] {accos} {csDesc} AT {rkopDesc} DOES {parseDesc} YIELD {outDesc}
            """);
    }

    public static CursorText ValidateToken(
        this CursorText text,
        TokenDescription description,
        string hint,
        out Token token,
        [CallerFilePath] string origin = "",
        [CallerLineNumber] int position = 0,
        [CallerMemberName] string member = "")
    {
        var continued = text.TakeToken(description, out token, origin, position, member);
        if (token.IsValid)
            return continued;
        else
        {
            GlobalLog.Instance?.Error("""
                Syntax error at {line}:{col}, 
                Expected: {description}
                {hint}
                """, text.GetCurrentLine(), text.GetCurrentCol(), description.HumanReadable, hint);
            throw new SyntaxException(text, $"""
                Syntax error at {text.GetCurrentLine()}:{text.GetCurrentCol()}, 
                Expected: {description.HumanReadable}
                {hint}
                """);

        }

    }
}
