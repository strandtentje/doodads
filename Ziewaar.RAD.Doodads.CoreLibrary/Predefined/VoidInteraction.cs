using static System.Array;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class VoidInteraction : IInteraction
{
    public static readonly VoidInteraction Instance = new VoidInteraction();
    public IInteraction Stack => Instance.Stack;
    public object Register => 0;
    public IReadOnlyDictionary<string, object> Memory => EmptyReadOnlyDictionary.Instance;
}
