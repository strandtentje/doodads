using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Ziewaar.RAD.Doodads.RKOP;

public class CursorText(
    DirectoryInfo workingDirectory,
    char[] text,
    int[] linePositions,
    CursorText scopeAbove,
    SortedList<string, object> localScope,
    int position = 0)
{
    public DirectoryInfo WorkingDirectory => workingDirectory;
    public char[] Text => text;
    public int[] LinePositions => linePositions;
    public int Position => position;
    public CursorText ScopeAbove => scopeAbove;
    public SortedList<string, object> LocalScope => localScope;
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
    public static CursorText Create(DirectoryInfo workingDirectory, string text)
    {
        List<int> linePositions = new();
        int currentPosition = 0;
        for (;
            currentPosition >= 0 &&
            currentPosition < text.Length;
            currentPosition = Math.Max(
                text.IndexOf('\n', currentPosition) + 1,
                text.IndexOf("\r\n", currentPosition) + 2))
        {
            if (linePositions.BinarySearch(currentPosition) >= 0)
                break;
            linePositions.Add(currentPosition);
        }
        return new CursorText(workingDirectory, text.ToCharArray(), linePositions.ToArray(), null, new(), 0);
    }
    public CursorText AdvanceTo(int position) => new CursorText(workingDirectory, Text, LinePositions, ScopeAbove, LocalScope, position);
    public CursorText EnterScope() => new CursorText(workingDirectory, Text, LinePositions, this, new SortedList<string, object>(), Position);
    public CursorText ExitScope() => ScopeAbove != null ?
        new CursorText(workingDirectory, Text, LinePositions, ScopeAbove.ScopeAbove, ScopeAbove.LocalScope, Position) :
        throw new InvalidOperationException("Cannot exit top scope.");
    public override string ToString() => $"@{this.GetCurrentLine()}:{this.GetCurrentCol()} ({(Position < Text.Length ? Text[Position] : "EOF")})";

}
