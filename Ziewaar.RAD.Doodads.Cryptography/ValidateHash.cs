using System.Diagnostics.CodeAnalysis;
using Isopoh.Cryptography.Argon2;
using Microsoft.DevTunnels.Ssh;
using Microsoft.DevTunnels.Ssh.Events;
using Microsoft.DevTunnels.Ssh.IO;
using Microsoft.DevTunnels.Ssh.Keys;
using Microsoft.DevTunnels.Ssh.Messages;
using Microsoft.DevTunnels.Ssh.Tcp;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;
using Ziewaar.RAD.Doodads.EnumerableStreaming.StreamingMultipart;
using Buffer = Microsoft.DevTunnels.Ssh.Buffer;
using ECDsa = Microsoft.DevTunnels.Ssh.Algorithms.ECDsa;

namespace Ziewaar.RAD.Doodads.Cryptography;

[Category("Tokens & Cryptography")]
[Title("Validate Password with Argon2")]
[Description("""
             When triggered with a sensitive interaction that's not been consumed (use LoadSensitive),
             And a password hash in the register,
             It will validate the sensitive string against the password hash.
             Argon2 is used.
             """)]
public class ValidateHash : IService
{
    [EventOccasion("When the hash and sensitive string matched")]
    public event CallForInteraction? OnThen;

    [EventOccasion("When the hash and sensitive string mismatched")]
    public event CallForInteraction? OnElse;

    [EventOccasion("When no sensitive string was found, or all of them were consumed")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ISensitiveInteraction>(out ISensitiveInteraction? sensitive,
                x => x.TryVirginity()) ||
            sensitive?.GetSensitiveObject()?.ToString() is not string sensitiveValue)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "can only validate sensitive strings; use LoadSensitive"));
            return;
        }

        if (Argon2.Verify(interaction.Register?.ToString(), sensitiveValue))
            OnThen?.Invoke(this, interaction);
        else
            OnElse?.Invoke(this, interaction);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class SshSessionOpened : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<SshServerInteraction>(out var serverInteraction) || serverInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH server interaction required"));
            return;
        }

        serverInteraction.SshServer.SessionOpened += (sender, session) =>
        {
            session.Authenticating += (o, args) =>
            {
                if (args.AuthenticationType != SshAuthenticationType.ClientPublicKey &&
                    args.AuthenticationType != SshAuthenticationType.ClientPublicKeyQuery)
                    args.AuthenticationTask =
                        Task.FromException<ClaimsPrincipal?>(new UnauthorizedAccessException("Only publickey allowed"));
            };
            OnThen?.Invoke(this, new SshSessionInteraction(interaction, session));
        };
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class SshSessionClosed : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<SshSessionInteraction>(out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH session interaction required"));
            return;
        }

        sessionInteraction.Session.Closed += (sender, session) =>
        {
            OnThen?.Invoke(this, interaction);
        };
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class CloseSshSession : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<SshSessionInteraction>(out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH session interaction required"));
            return;
        }

        if (!sessionInteraction.Session.IsClosed)
        {
            try
            {
                sessionInteraction.Session.CloseAsync(SshDisconnectReason.ByApplication).Wait();
                OnThen?.Invoke(this, interaction);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, ex));
            }
        }
        else
        {
            OnElse?.Invoke(this, interaction);
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class SshSessionPublicKeyQuery : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!(constants, RepeatNameConstant).IsRereadRequired(out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (string.IsNullOrWhiteSpace(this.CurrentRepeatName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
            return;
        }

        if (!interaction.TryGetClosest<SshSessionInteraction>(out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH session interaction required"));
            return;
        }

        var formatter = new Pkcs8KeyFormatter();

        sessionInteraction.Session.Authenticating += (sender, args) =>
        {
            if (args.AuthenticationType != SshAuthenticationType.ClientPublicKeyQuery || args.PublicKey == null)
                return;
            var pem = formatter.Export(args.PublicKey, includePrivate: false).EncodePem();
            var repeatInteraction = new RepeatInteraction(this.CurrentRepeatName, interaction) { IsRunning = false };
            var pemInteraction = new ClaimsSinkingInteraction(repeatInteraction, [new Claim("publickeypem", pem)]);
            OnThen?.Invoke(this, pemInteraction);
            if (repeatInteraction.IsRunning)
            {
                var claimsIdentity = new ClaimsIdentity(pemInteraction.Claims);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                args.AuthenticationTask = Task.FromResult<ClaimsPrincipal?>(claimsPrincipal);
            }
        };
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class ReadClaim : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ClaimsSinkingInteraction>(out var claimsInteraction) ||
            claimsInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Claims interaction required"));
            return;
        }

        var claim = claimsInteraction.Claims.SingleOrDefault(x => x.Type == interaction.Register.ToString());

        if (claim == null) OnElse?.Invoke(this, interaction);
        else OnElse?.Invoke(this, new CommonInteraction(interaction, claim.Value));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class ChangeClaim : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ClaimsSinkingInteraction>(out var claimsInteraction) ||
            claimsInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Claims interaction required"));
            return;
        }

        if (interaction.Register.ToString() is not string claimType)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Claim type required"));
            return;
        }

        var tsi = new TextSinkingInteraction(interaction);
        OnThen?.Invoke(this, tsi);
        var newValue = tsi.ReadAllText();
        
        if (claimsInteraction.Claims.SingleOrDefault(x => x.Type == claimType) is
            { } existingClaim)
            claimsInteraction.Claims.Remove(existingClaim);
        claimsInteraction.Claims.Add(new (claimType ?? string.Empty, newValue));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class SshSessionPublicKeyAuthentic : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<SshSessionInteraction>(out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH session interaction required"));
            return;
        }

        var formatter = new Pkcs8KeyFormatter();

        sessionInteraction.Session.Authenticating += (sender, args) =>
        {
            if (args.AuthenticationType != SshAuthenticationType.ClientPublicKey || args.PublicKey == null)
                return;
        };
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class ClaimsSinkingInteraction(IInteraction interaction, IList<Claim> claims) : IInteraction
{
    public IInteraction Stack => interaction.Stack;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public IList<Claim> Claims => claims;
}

public class SshSessionInteraction(IInteraction interaction, SshServerSession session) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;

    public IReadOnlyDictionary<string, object> Memory { get; } = new SwitchingDictionary(
        ["remotehost", "remoteversion", "remoteprotocol"], key =>
            key switch
            {
                "remotehost" => (session.RemoteEndpoint as IPEndPoint)?.Address?.ToString() ?? "",
                "remoteversion" => session.RemoteVersion?.Name ?? "",
                "remoteprotocol" => session.RemoteVersion?.ProtocolVersion.ToString() ?? "",
                _ => throw new KeyNotFoundException(),
            });

    public SshServerSession Session => session;
}

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

public static class KeyExportUtilities
{
    string ExportPrivateKeyPem(ECDsa.KeyPair keyPair)
    {
        var ecdsaKeypair = ECDsaOpenSsl.Create(keyPair.ExportParameters(true));
        var privateKeyBytes = ecdsaKeypair.ExportPkcs8PrivateKey();
        byte[] pkcs8 = ecdsaKeypair.ExportPkcs8PrivateKey();
        string b64 = Convert.ToBase64String(pkcs8, Base64FormattingOptions.InsertLineBreaks);
        return $"-----BEGIN PRIVATE KEY-----\n{b64}\n-----END PRIVATE KEY-----\n";
    }

// Import back from PEM
    KeyPair ImportPrivateKeyPem(string pem)
    {
        var rsa = RSA.Create();
        string b64 = pem.Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("\n", "")
            .Trim();
        byte[] pkcs8 = Convert.FromBase64String(b64);
        rsa.ImportPkcs8PrivateKey(pkcs8, out _);

        var rsaParams = rsa.ExportParameters(true);
        return new KeyPair(KeyAlgorithm.Rsa, rsaParams);
    }
}