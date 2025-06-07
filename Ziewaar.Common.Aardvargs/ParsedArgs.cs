using System;
using System.Collections.Generic;

namespace Ziewaar.Common.Aardvargs;

public class ParsedArgs
{
    public List<string> Filenames { get; } = new();
    public SortedList<string, object> Options { get; } = new();

    public void Dump()
    {
        Console.WriteLine("Filenames:");
        foreach (var file in Filenames)
            Console.WriteLine($"  {file}");

        Console.WriteLine("Options:");
        foreach (var opt in Options)
            Console.WriteLine($"  {opt.Key}: {opt.Value} ({opt.Value.GetType().Name})");
    }
}
