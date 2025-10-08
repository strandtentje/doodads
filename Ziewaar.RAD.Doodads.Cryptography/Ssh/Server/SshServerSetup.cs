using System.Diagnostics;
using Ziewaar.RAD.Doodads.Cryptography.Keypairs;

#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Server;

[Category("Networking & Connections")]
[Title("Setup SSH server")]
[Description("""
             Prepares an SSH server with previously configured server
             credentials (use the Ecdsa services)
             """)]
public class SshServerSetup : IService
{
    [PrimarySetting("Under what name to daemonize")]
    private readonly UpdatingPrimaryValue DaemonNameConst = new();
    private string? CurrentDaemonName;
    [EventOccasion("""
                   A working but non-listening SSH server comes out here 
                   (and it'll do that again if it was daemonized and broke down)
                   """)]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When there was no server identity keypair, or the daemon name was incomplete.")]
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