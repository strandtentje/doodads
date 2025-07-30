namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services
{
#pragma warning disable 67
    public interface IControlCommandReceiver<TCommandEnum> : IDisposable where TCommandEnum : struct, Enum, IConvertible
    {
        void GiveCommand(TCommandEnum command);
        TCommandEnum CurrentState { get; }
    }
}