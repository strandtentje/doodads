namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class VoidInteraction : IInteraction
{
    private VoidInteraction()
    {

    }
    public static readonly VoidInteraction Instance = new VoidInteraction();
    public IInteraction Parent => null;
    public IReadOnlyDictionary<string, object> Variables => new SortedList<string, object>();
}
