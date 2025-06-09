namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface ICheckUpdateRequiredInteraction : IInteraction
{
    ISinkingInteraction Original { get; }
    bool IsRequired { get; set; }
}