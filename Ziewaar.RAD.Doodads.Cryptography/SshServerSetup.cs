using Microsoft.DevTunnels.Ssh;
using Microsoft.DevTunnels.Ssh.Tcp;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Doodads.Cryptography;

public class SshServerSetup : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        SshSessionConfiguration config = new() { AuthenticationMethods = { "publickey" } };
        using var server = new SshServer(config, SerilogTraceListener.CreateDebug<SshServer>(GlobalLog.Instance));
        OnThen?.Invoke(this, new SshServerInteraction(interaction, server));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}