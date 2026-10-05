using System;
using System.Collections.Generic;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Data.FlatFile.Support;

namespace Ziewaar.RAD.Doodads.Data.MemoryStore;

public abstract class MemoryStore : BasicService
{
    internal static readonly object StoreLock = new object();
    internal static readonly SortedList<string[], FlatFileMemberMemory> Store = new SortedList<string[], FlatFileMemberMemory>(
        new StringArrayComparer(StringComparer.Ordinal));
}
