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
using System.Security.Claims;
using System.Security.Cryptography;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
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

public class SshSessionTcpIpForwardingOffer : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnAlternativePort;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<SshSessionInteraction>(out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH session interaction required"));
            return;
        }
        
        sessionInteraction.Session.Request += (sender, args) =>
        {
            if (args.Request is not SessionRequestMessage requestMessage)
                return;
            if (args.RequestType != "tcpip-forward")
                return;

            var portForwardRequestMessage = requestMessage.ConvertTo<PortForwardRequestMessage>();

            var claimsSourcingInteraction = new ClaimsSourcingInteraction(interaction, args.Principal);

            var tsi = new TextSinkingInteraction(claimsSourcingInteraction);
            OnAlternativePort?.Invoke(this, tsi);
            var alternativePortText = tsi.ReadAllText();
            uint definitivePort = portForwardRequestMessage.Port;
            if (!string.IsNullOrWhiteSpace(alternativePortText) &&
                ushort.TryParse(alternativePortText, out var alternatvePortValue))
                definitivePort = alternatvePortValue;

            if (args.Request.WantReply && portForwardRequestMessage.Port == 0)
                args.ResponseTask = Task.FromResult<SshMessage>(new SshPfwOkMessage(definitivePort));

            OnThen?.Invoke(this, new CommonInteraction(claimsSourcingInteraction, new SwitchingDictionary(
                ["tcpip-forward-ip", "tcpip-forward-port", "definitive-port"], key => key switch
                {
                    "tcpip-forward-ip" => portForwardRequestMessage.AddressToBind ?? "",
                    "tcpip-forward-port" => portForwardRequestMessage.Port,
                    "definitive-port" => definitivePort,
                    _ => throw new KeyNotFoundException(),
                })));
        };
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class SetSshSessionShelved : IService
{
    internal static readonly SortedList<Guid, SshServerSession> ShelvedSessions = new();
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

        var shelfGuid = Guid.NewGuid();

        void RemoveShelvedSession(object? sender, SshSessionClosedEventArgs args)
        {
            sessionInteraction.Session.Closed -= RemoveShelvedSession;
            ShelvedSessions.Remove(shelfGuid);
        }

        sessionInteraction.Session.Closed += RemoveShelvedSession;
        if (sessionInteraction.Session.IsClosed)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Cannot shelve closed sessions"));
            sessionInteraction.Session.Closed -= RemoveShelvedSession;
            return;
        }
        else
        {
            ShelvedSessions.Add(shelfGuid, sessionInteraction.Session);
            var claimsInteraction = new ClaimsSourcingInteraction(interaction, sessionInteraction.Session.Principal);
            OnThen?.Invoke(this, new CommonInteraction(claimsInteraction, register: shelfGuid.ToString()));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class GetShelvedSshSession : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        string? guidString = interaction.Register.ToString();
        if (string.IsNullOrWhiteSpace(guidString))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction,"guid required in register to get shelved ssh session"));
            return;
        }
        var shelvedGuid = Guid.Parse(guidString);
        if (SetSshSessionShelved.ShelvedSessions.TryGetValue(shelvedGuid, out var session))
        {
            var claimsInteraction = new ClaimsSourcingInteraction(interaction, session.Principal);
            OnThen?.Invoke(this, new SshSessionInteraction(claimsInteraction, session));
        }
        else 
            OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class SshSessionTcpIpForwardingConnection : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnEndpoint;
    public event CallForInteraction? OnOrigin;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<SshSessionInteraction>(out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH session interaction required"));
            return;
        }

        var claimsInteraction = new ClaimsSourcingInteraction(interaction, sessionInteraction.Session.Principal);
        var endpointSink = new TextSinkingInteraction(claimsInteraction);
        OnEndpoint?.Invoke(this, endpointSink);
        if (!IPEndPoint.TryParse(endpointSink.ReadAllText(), out var connectedEndpoint))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Badly formatted connected endpoint"));
            return;
        }

        var originSink = new TextSinkingInteraction(claimsInteraction);
        OnOrigin?.Invoke(this, originSink);
        if (!IPEndPoint.TryParse(connectedEndpoint.Address.ToString(), out var originEndpoint))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Badly formatted origin endpoint"));
            return;
        }
        
        
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class ClaimsSourcingInteraction(IInteraction interaction, ClaimsPrincipal? argsPrincipal)
    : IClaimsReadingInteraction
{
    public IInteraction Stack => interaction;
    public object Register => Stack.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public IEnumerable<Claim> Claims { get; } = argsPrincipal?.Claims ?? [];
}

public class SshPfwOkMessage(uint actualPort) : SshMessage
{
    public override byte MessageType => 81;

    protected override void OnRead(ref SshDataReader reader)
    {
        throw new NotSupportedException();
    }

    protected override void OnWrite(ref SshDataWriter writer)
    {
        writer.Write(actualPort);
    }
}

public class SshPfwReqMessage(
    string serverGotConnectionOnAddress,
    uint serverGotConnectionOnPort,
    string serverGotConnectionFromAddress,
    uint serverGotConnectionFromPort)
    : ChannelOpenMessage
{
    public override byte MessageType => 90;

    protected override void OnRead(ref SshDataReader reader)
    {
        throw new NotSupportedException();
    }

    protected override void OnWrite(ref SshDataWriter writer)
    {
        writer.Write("forwarded-tcpip", Encoding.ASCII);
        writer.Write(SenderChannel);
        writer.Write(SshChannel.DefaultMaxWindowSize);
        writer.Write(SshChannel.DefaultMaxPacketSize);
        writer.Write(serverGotConnectionOnAddress, Encoding.ASCII);
        writer.Write(serverGotConnectionOnPort);
        writer.Write(serverGotConnectionFromAddress, Encoding.ASCII);
        writer.Write(serverGotConnectionFromPort);
    }
}

public class ReadClaim : IService
{
    private readonly UpdatingPrimaryValue ClaimTypeOverrideConst = new();
    private string? CurrentClaimTypeOverride;

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public virtual void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ClaimTypeOverrideConst).IsRereadRequired(out string? candidateClaimType))
            this.CurrentClaimTypeOverride = candidateClaimType;

        if (!interaction.TryGetClosest<IClaimsReadingInteraction>(out var claimsInteraction) ||
            claimsInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Claims interaction required"));
            return;
        }

        string claimType;
        if (this.CurrentClaimTypeOverride is string overrideType)
            claimType = overrideType;
        else if (interaction.Register.ToString() is string dynamicType)
            claimType = dynamicType;
        else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Claim type required"));
            return;
        }

        var claim = claimsInteraction.Claims.SingleOrDefault(x => x.Type == claimType);

        if (claim == null) OnElse?.Invoke(this, interaction);
        else OnThen?.Invoke(this, new CommonInteraction(interaction, claim.Value));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class ReadNameClaim : ReadClaim
{
    public override void Enter(StampedMap constants, IInteraction interaction) =>
        base.Enter(new StampedMap(ClaimsIdentity.DefaultNameClaimType), interaction);
}

public class ReadRoleClaim : ReadClaim
{
    public override void Enter(StampedMap constants, IInteraction interaction) =>
        base.Enter(new StampedMap(ClaimsIdentity.DefaultNameClaimType), interaction);
}

public class ChangeClaim : IService
{
    private readonly UpdatingPrimaryValue ClaimTypeOverrideConst = new();
    private string? CurrentClaimTypeOverride;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public virtual void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ClaimTypeOverrideConst).IsRereadRequired(out string? candidateClaimType))
            this.CurrentClaimTypeOverride = candidateClaimType;

        if (!interaction.TryGetClosest<ClaimsSinkingInteraction>(out var claimsInteraction) ||
            claimsInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Claims interaction required"));
            return;
        }

        string claimType;
        if (this.CurrentClaimTypeOverride is string overrideType)
            claimType = overrideType;
        else if (interaction.Register.ToString() is string dynamicType)
            claimType = dynamicType;
        else
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
        claimsInteraction.Claims.Add(new(claimType ?? string.Empty, newValue));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class ChangeNameClaim : ChangeClaim
{
    public override void Enter(StampedMap constants, IInteraction interaction) =>
        base.Enter(new StampedMap(ClaimsIdentity.DefaultNameClaimType), interaction);
}

public class ChangeRoleClaim : ChangeClaim
{
    public override void Enter(StampedMap constants, IInteraction interaction) =>
        base.Enter(new StampedMap(ClaimsIdentity.DefaultRoleClaimType), interaction);
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

public class ClaimsSinkingInteraction(IInteraction interaction, IList<Claim> claims)
    : IInteraction, IClaimsReadingInteraction
{
    public IInteraction Stack => interaction.Stack;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public IList<Claim> Claims => claims;
    IEnumerable<Claim> IClaimsReadingInteraction.Claims => claims;
}

public interface IClaimsReadingInteraction : IInteraction
{
    IEnumerable<Claim> Claims { get; }
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