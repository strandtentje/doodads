#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;

public class SplitterSetup(string[] items, string splitter)
{
    public string[] Items { get; } = items;
    public string Splitter { get; } = splitter;
}