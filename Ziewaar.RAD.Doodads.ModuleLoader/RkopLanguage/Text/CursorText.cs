using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Doodads.ModuleLoader.RkopLanguage.Text;

public class CursorText(
    DirectoryInfo workingDirectory,
    string bareFileName,
    char[] text,
    CursorText scopeAbove,
    SortedList<string, object> localScope,
    IReadOnlyDictionary<string, object> policies,
    int position = 0)
{
    public bool LogFileExists { get; } = File.Exists($"{bareFileName}.log");
    private string LogFilePath { get; } = $"{bareFileName}.log";
    public IReadOnlyDictionary<string, object> Policies => policies;

    public void EnqueueLogMessage(string message)
    {
        if (LogFileExists)
            File.AppendAllLines(LogFilePath, [message]);
    }

    private static readonly CursorText FixedEmpty = Create(
        new DirectoryInfo(Environment.CurrentDirectory),
        "deleted", "");

    public static readonly CursorText Empty = FixedEmpty.AdvanceTo(0);

    public List<string> Stack
    {
        get
        {
            if (scopeAbove != null && scopeAbove != this)
                return scopeAbove.Stack;
            else
                return field;
        }
    } = [];

    public int Depth
    {
        get
        {
            if (scopeAbove == null || scopeAbove == this) return 0;
            else return scopeAbove.Depth + 1;
        }
    }

    public string LastIdentifier
    {
        get
        {
            if (scopeAbove != null && scopeAbove != this)
                return scopeAbove.LastIdentifier;
            else
                return field;
        }
        set
        {
            if (scopeAbove != null && scopeAbove != this)
                scopeAbove.LastIdentifier = value;
            else
                field = value;
        }
    } = "Root";

    public DirectoryInfo WorkingDirectory => workingDirectory;
    public char[] Text => text;
    public int Position => position;
    public CursorText ScopeAbove => scopeAbove;
    public SortedList<string, object> LocalScope => localScope;

    public string BareFile => bareFileName;

    public object this[string key]
    {
        get => LocalScope.TryGetValue(key, out var value) ? value : ScopeAbove?[key];
        set => LocalScope[key] = value;
    }

    public static CursorText Create(DirectoryInfo workingDirectory, string bareFileName,
        string text) =>
        new(workingDirectory, bareFileName, text.ToCharArray(), null, new(),
            EmptyReadOnlyDictionary.Instance);

    public CursorText IncludePolicy(string name, object value) =>
        new CursorText(workingDirectory, bareFileName, text, scopeAbove, localScope,
            new FallbackReadOnlyDictionary(
                new SwitchingDictionary([name],
                    key => key == name ? value : throw new KeyNotFoundException()), policies),
            position);

    public CursorText AdvanceTo(int advancedPosition) =>
        new CursorText(workingDirectory, bareFileName, Text, ScopeAbove, LocalScope, policies,
            advancedPosition);

    public CursorText EnterScope() => new CursorText(workingDirectory, bareFileName, Text, this,
        new SortedList<string, object>(), policies, Position);

    public CursorText ExitScope() => ScopeAbove != null
        ? new CursorText(workingDirectory, bareFileName, Text, ScopeAbove.ScopeAbove,
            ScopeAbove.LocalScope, policies,
            Position)
        : throw new InvalidOperationException("Cannot exit top scope.");

    public override string ToString() =>
        $"@{this.GetCurrentLine()}:{this.GetCurrentCol()} ({(Position < Text.Length ? Text[Position] : "EOF")})";
}