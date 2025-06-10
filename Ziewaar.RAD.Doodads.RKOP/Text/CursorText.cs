namespace Ziewaar.RAD.Doodads.RKOP.Text;

public class CursorText(
    DirectoryInfo workingDirectory,
    string bareFileName,
    char[] text,
    CursorText scopeAbove,
    SortedList<string, object> localScope,
    int position = 0)
{
    private static readonly CursorText FixedEmpty = Create(
        new DirectoryInfo(Environment.CurrentDirectory),
        "deleted", "");
    public static CursorText Empty = FixedEmpty.AdvanceTo(0);
    public DirectoryInfo WorkingDirectory => workingDirectory;
    public char[] Text => text;    
    public int Position => position;
    public CursorText ScopeAbove => scopeAbove;
    public SortedList<string, object> LocalScope => localScope;

    public string BareFile => bareFileName;

    public object this[string key]
    {
        get
        {
            if (LocalScope.TryGetValue(key, out var value))
                return value;
            else if (ScopeAbove != null)
                return ScopeAbove[key];
            else
                return null;
        }
        set
        {
            LocalScope[key] = value;
        }
    }
    public static CursorText Create(DirectoryInfo workingDirectory, string bareFileName, string text)
    {
        return new CursorText(workingDirectory, bareFileName, text.ToCharArray(), null, new(), 0);
    }
    public CursorText AdvanceTo(int position) => new CursorText(workingDirectory, bareFileName, Text, ScopeAbove, LocalScope, position);
    public CursorText EnterScope() => new CursorText(workingDirectory, bareFileName, Text, this, new SortedList<string, object>(), Position);
    public CursorText ExitScope() => ScopeAbove != null ?
        new CursorText(workingDirectory, bareFileName, Text, ScopeAbove.ScopeAbove, ScopeAbove.LocalScope, Position) :
        throw new InvalidOperationException("Cannot exit top scope.");
    public override string ToString() => $"@{this.GetCurrentLine()}:{this.GetCurrentCol()} ({(Position < Text.Length ? Text[Position] : "EOF")})";

}
