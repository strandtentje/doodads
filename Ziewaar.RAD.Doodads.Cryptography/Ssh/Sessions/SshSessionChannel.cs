using Microsoft.DevTunnels.Ssh.Tcp;

namespace Ziewaar.RAD.Doodads.Cryptography;
public class SshSessionChannel : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
    }
    private void HandleChannel(SshServer server)
    {
    }
    public void HandleFatal(IInteraction source, Exception ex)
    {
    }
}