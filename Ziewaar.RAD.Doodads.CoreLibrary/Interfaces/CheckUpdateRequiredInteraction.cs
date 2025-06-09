namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public class CheckUpdateRequiredInteraction(IInteraction parent, ISinkingInteraction previous) : ICheckUpdateRequiredInteraction
{
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public ISinkingInteraction Original => previous;
    public bool IsRequired { get; set; } = false;
}