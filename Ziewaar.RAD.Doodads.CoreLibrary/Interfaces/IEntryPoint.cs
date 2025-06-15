namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface IEntryPoint
{
    [DebuggerHidden]
    void Run(object sender, IInteraction interaction);
}