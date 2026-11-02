#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;

public class SplitterSetup(string repeatName, string[] items, string splitter)
{
    public string RepeatName { get; } = repeatName;
    public string[] Items { get; } = items;
    public string Splitter { get; } = splitter;
}