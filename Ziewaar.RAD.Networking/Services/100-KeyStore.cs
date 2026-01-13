using Serilog;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Networking;

public abstract class KeyStore<TType> : IService
{
    private readonly UpdatingPrimaryValue DirectoryConstant = new UpdatingPrimaryValue();

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (GlobalLog.Instance is not ILogger logger)
            OnException?.Invoke(this, interaction.AppendRegister("Logger required for this"));
        else if (constants.PrimaryConstant.ToString() is not string keyDir)
            OnException?.Invoke(this, interaction.AppendRegister("key directory must be configured"));
        else
            OnThen?.Invoke(this, new CustomInteraction<TType>(interaction, Create(logger, keyDir)));
    }

    protected abstract TType Create(ILogger logger, string keyDir);
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}