namespace Ziewaar.RAD.Doodads.RKOP.Text;

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
        OnThenShorthand = new TokenDescription((pos, chr) => pos switch
        {
            0 => chr == '.' || chr == ':',
            _ => false
        }, str => str == "." || str == ":", "Period or colon in block to indicate OnThen"),        
        OnElseShorthand = new TokenDescription((pos, chr) => pos switch
        {
            0 => chr == ',' || chr == '|',
            _ => false
        }, str => str == "," || str == "|", "Comma or pipe in block to indicate OnElse"),
        OnAnythingShorthand = new TokenDescription((pos, chr) => pos switch
        {
            0 => chr == '÷',
            _ => false
        }, str => str == "÷", "Obelus character to indicate any branch"),
        Wiggly =  DescribeSingleCharacter('~', "Case Shorthand (~)"),
        LoadShorthand = DescribeSingleCharacter('?', "Load variable question"),
        HatShorthand = DescribeSingleCharacter('^', "Hat sign"),
        StoreShorthand = DescribeSingleCharacter('!', "Store variable exclamation"),
        FormatShorthand = DescribeSingleCharacter('$', "Format string $"),
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
        BeakOpen = DescribeSingleCharacter('<', "Start with open beak"),
        BeakClose = DescribeSingleCharacter('>', "End with closed beak"),
        AmpersandP = DescribeSingleCharacter('&', "Et sign"),
        Pipe = DescribeSingleCharacter('|', "Or pipe"),
        Obelus = DescribeSingleCharacter('÷', "Or/And Obelus"),
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
            }, x => x.Length == 2, "Announcement of filename string with [f\"], which evaluates relative to the current directory."),

        TemplatePathAnnouncement = new TokenDescription(
            (pos, chr) => pos switch
            {
                0 => chr == 't',
                1 => chr == '"',
                _ => false,
            }, x => x.Length == 2, "Announcement of filename string with [t\"], which evaluates into the `templates` dir relative to the current directory."),

        QueryPathAnnouncement = new TokenDescription(
            (pos, chr) => pos switch
            {
                0 => chr == 'q',
                1 => chr == '"',
                _ => false,
            }, x => x.Length == 2, "Announcement of filename string with [q\"], which evaluates into the `queries` dir relative to the current directory."),

        ConfigurationPathAnnouncement = new TokenDescription(
            (pos, chr) => pos switch
            {
                0 => chr == 'c',
                1 => chr == '"',
                _ => false,
            }, x => x.Length == 2, "Announcement of filename string with [c\"] which evaluates to the OS configuration dir ie. %APPDATA% or ~/.config"),

        ProfilePathAnnouncement = new TokenDescription(
            (pos, chr) => pos switch
            {
                0 => chr == 'p',
                1 => chr == '"',
                _ => false,
            }, x => x.Length == 2, "Announcement of filename string with [p\"] which evaluates to the current user profile dir ie. `%HOMEDRIVE%%HOMEPATH%` or `~`"),

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
        MaybeNegativeNumbers = new TokenDescription(
            (pos, chr) => pos switch
            {
        0 => char.IsDigit(chr) || chr == '-',
                _ => char.IsDigit(chr)
            }, x => x.Length > 0, "Numbers 0-9 optionally starting with -"),
        DecimalSeparator = DescribeSingleCharacter('.', "Decimal point (.)");

}
