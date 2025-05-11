using Ziewaar.RAD.Doodads.CoreLibrary.Attributes;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;


public interface IService
{
    [NamedBranch] public event EventHandler<IInteraction> OnError;
    void Enter(
        ServiceConstants serviceConstants,
        IInteraction interaction);
}