namespace Ziewaar.RAD.Doodads.RKOP;

public class TokenDescription(
    Func<int, char, bool> predicate,
    Func<string, bool> validation,
    string humanReadable)
{
    public bool Predicate(int position, char character) => predicate(position, character);
    public bool Validate(string text) => validation(text);
    public string HumanReadable => humanReadable;
    public static readonly TokenDescription IdentifierWithoutUnderscore = new TokenDescription(
        (pos, chr) => pos switch
        {
            0 => char.IsLetter(chr),
            _ => char.IsLetterOrDigit(chr) || chr == '_',
        },
        x => x.Length > 0,
        "Identifier starting with letter and continuing with letter, digit or underscore"
    );
    public static readonly TokenDescription Identifier = new TokenDescription(
        (pos, chr) => pos switch
        {
            0 => char.IsLetter(chr) || chr == '_',
            _ => char.IsLetterOrDigit(chr) || chr == '_',
        },
        x => x.Length > 0,
        "Identifier starting with letter and continuing with letter, digit or underscore"
    );
    public static TokenDescription DescribeSingleCharacter(char expectedCharacter, string humanReadable) => new TokenDescription(
        (pos, chr) => pos switch
        {
            0 => chr == expectedCharacter,
            _ => false
        }, x => x.Length == 1,
        humanReadable);
    public static readonly TokenDescription
        Underscore = DescribeSingleCharacter('_', "Underscore"),
        StartOfArguments = DescribeSingleCharacter('(', "Opening bracket ("),
        EndOfArguments = DescribeSingleCharacter(')', "Closing bracket )"),
        ArgumentSeparator = DescribeSingleCharacter(',', "List separator (comma, ',')"),
        AssignmentOperator = DescribeSingleCharacter('=', "Assignment operator (equals, '=')"),
        Terminator = DescribeSingleCharacter(';', "Terminator char (semicol, ';')"),
        DefaultBranchCoupler = DescribeSingleCharacter(':', "Coupler char (col, ':')"),
        TermOrAmpP = new TokenDescription(
            (pos, chr) => pos switch
            {
                0 => chr == ';' || chr == '&' || chr == '|',
                _ => false,
            }, x => x.Length == 1, "Termining semicol or ampersand for next service"),
        ChainerP = new TokenDescription(
            (pos, chr) => pos switch
            {
                0 => chr == ';' || chr == ':' || chr == '&' || chr == '|',
                _ => false
            }, x => x.Length == 1, "What to do after this description"),
        BlockOpen = DescribeSingleCharacter('{', "Open curly bracket"),
        BlockClose = DescribeSingleCharacter('}', "Close curly bracket"),
        ArrayOpen = DescribeSingleCharacter('[', "Start of array with blocky bracket"),
        ArrayClose = DescribeSingleCharacter(']', "End of array with blocky bracket"),
        AmpersandP = DescribeSingleCharacter('&', "Et sign"),
        Pipe = DescribeSingleCharacter('|', "Or pipe"),
        TrueOrFalse = new TokenDescription(
            (pos, chr) =>
            {
                var uchar = char.ToUpper(chr);
                return
                    uchar == "FALSE".ElementAtOrDefault(pos) ||
                    uchar == "TRUE".ElementAtOrDefault(pos);
            }, x => bool.TryParse(x, out var _), "expected true or false"),
        NextOrCloseArray = new TokenDescription(
            (pos, chr) => pos switch
            {
                0 => chr == ']' || chr == ',',
                _ => false,
            }, x => x.Length == 1, "End of array with ], or next item with comma"),
        RelativePathAnnouncement = new TokenDescription(
            (pos, chr) => pos switch
            {
                0 => chr == 'f',
                1 => chr == '"',
                _ => false,
            }, x => x.Length == 2, "Announcement of filename string with [f\"]"),
        BranchAnnouncement = new TokenDescription(
            (pos, chr) => pos switch
            {
                0 => chr == '-',
                1 => chr == '>',
                _ => false,
            }, x => x.Length == 2, "Announcement of branch after identifier using ->"),
        DoubleQuotes = DescribeSingleCharacter('"', "String double quotes (\")"),
        StringGuts = new TokenDescription(
            (pos, chr) => chr != '"' && chr != '\\',
            x => true, "Reasonable string contents"),
        EscapeSequence = new TokenDescription(
            (pos, chr) => pos switch
            {
                0 => chr == '\\',
                1 => true,
                _ => false,
            }, x => x.Length == 2, "Escape sequence"),
        Numbers = new TokenDescription(
            (pos, chr) => char.IsDigit(chr),
            x => x.Length > 0, "Numbers 0-9"),
        DecimalSeparator = DescribeSingleCharacter('.', "Decimal point (.)");

}
