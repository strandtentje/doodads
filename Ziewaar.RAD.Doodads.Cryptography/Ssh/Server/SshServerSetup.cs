using Microsoft.DevTunnels.Ssh;
using Microsoft.DevTunnels.Ssh.Tcp;
using System.Diagnostics;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.Cryptography.Keypairs;
using Ziewaar.RAD.Doodads.Cryptography.Ssh.Server.Support;

#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Server;

public class SshServerSetup : IService
{
    private readonly UpdatingPrimaryValue DaemonNameConst = new();
    private string? CurrentDaemonName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, DaemonNameConst).IsRereadRequired(out string? candidateDaemonName))
            this.CurrentDaemonName = candidateDaemonName;
        if (string.IsNullOrWhiteSpace(this.CurrentDaemonName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Daemon name required"));
            return;
        }

        if (!interaction.TryGetClosest<EcdsaPrivateKeyInteraction>(out var serverIdentityInteraction)
            || serverIdentityInteraction == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "Server identity interaction not found (ecdsa)"));
            return;
        }

        SshSessionConfiguration config = new() { AuthenticationMethods = { "publickey" } };

        var ri = new RepeatInteraction(this.CurrentDaemonName, interaction) { IsRunning = true };

        do
        {
            using var server = new SshServer(config, new TraceSource("ssh", SourceLevels.Information));
            server.Credentials = new SshServerCredentials([serverIdentityInteraction.PrivateKey]);
            OnThen?.Invoke(this, new SshServerInteraction(ri, server));
        } while (ri.IsRunning);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}