using System.Collections.Concurrent;
using System.Threading;

namespace Ziewaar.RAD.Doodads.RKOP.Text;

public class CursorText(
    DirectoryInfo workingDirectory,
    string bareFileName,
    char[] text,
    CursorText scopeAbove,
    SortedList<string, object> localScope,
    int position = 0)
{
    public bool LogFileExists { get; } = File.Exists($"{bareFileName}.log");
    public string LogFilePath { get; } = $"{bareFileName}.log";

    private readonly object FileLock = new();
    private Timer LogFlusher = null;
    private Queue<string> Messages = null;
    private int LogCounter = 0;
    public void EnqueueLogMessage(string message)
    {
        if (LogFileExists)
            File.AppendAllLines(LogFilePath, [message]);
    }

    private static readonly CursorText FixedEmpty = Create(
        new DirectoryInfo(Environment.CurrentDirectory),
        "deleted", "");
    public static CursorText Empty = FixedEmpty.AdvanceTo(0);
    List<string> localStack = new List<string>();
    public List<string> Stack
    {
        get
        {
            if (scopeAbove != null && scopeAbove != this)
                return scopeAbove.Stack;
            else
                return localStack;
        }
    }

    public int Depth
    {
        get
        {
            if (scopeAbove == null || scopeAbove == this) return 0;
            else return scopeAbove.Depth + 1;
        }
    }

    string lifield = "Root";
    public string LastIdentifier
    {
        get
        {
            if (scopeAbove != null && scopeAbove != this)
                return scopeAbove.LastIdentifier;
            else
                return lifield;
        }
        set
        {
            if (scopeAbove != null && scopeAbove != this)
                scopeAbove.LastIdentifier = value;
            else
                lifield = value;
        }
    }

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
