using System.Collections.Generic;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Data.Services;

public class BufferedDataInteraction(
    IInteraction parent,
    IReadOnlyDictionary<string, object> cells,
    int currentRow,
    int totalRows) : IInteraction
{
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public int CurrentRow => currentRow;
    public int TotalRows => totalRows;

    public IReadOnlyDictionary<string, object> Memory { get; } = new FallbackReadOnlyDictionary(
        new SwitchingDictionary(["query_current_row", "query_total_rows"], key => key switch
        {
            "query_current_row" => currentRow,
            "query_total_rows" => totalRows,
            _ => throw new KeyNotFoundException(),
        }), cells);
}