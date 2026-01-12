using Serilog;
using System.Net.Sockets;
using System.Security.Cryptography;
using Ziewaar.Network.Encryption;
using Ziewaar.Network.Protocol;
using Ziewaar.Network.Tunneling;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Networking;

public class EncryptedDuplex : IService
{
    private readonly UpdatingPrimaryValue TestNameConstant = new();
    public event CallForInteraction? OnNameOffer;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<CustomInteraction<(TcpClient TcpClient, ProtocolOverStream Protocol)>>(
                out var connectionInteraction) ||
            connectionInteraction == null)
            OnException?.Invoke(this,
                interaction.AppendRegister("This services requires a live protocol to've been setup before it"));
        else if (!interaction.TryGetClosest<CustomInteraction<PublicKeyRepository>>(
                     out var publicKeyStoreInteraction) ||
                 publicKeyStoreInteraction == null)
            OnException?.Invoke(this, interaction.AppendRegister("This service requires a public key store"));
        else if (!interaction.TryGetClosest<CustomInteraction<RsaWrapper>>(out var selectedPrivateKeyInteraction) ||
                 selectedPrivateKeyInteraction == null)
            OnException?.Invoke(this, interaction.AppendRegister("This service requires a private key to be selected"));
        else if (GlobalLog.Instance is not ILogger logger)
            OnException?.Invoke(this, interaction.AppendRegister("This service requires a runtime with a logger"));
        else if (interaction.Register is not string localIdentifier || string.IsNullOrWhiteSpace(localIdentifier))
            OnException?.Invoke(this, interaction.AppendRegister("Local identifier expected in register"));
        else if (constants.PrimaryConstant.ToString() is not string repeatName || string.IsNullOrWhiteSpace(repeatName))
            OnException?.Invoke(this,
                interaction.AppendRegister("Service requires name as primary setting to confirm incoming names on."));
        else
        {
            var ct = interaction.TryGetClosest<CancellationInteraction>(out var cancellationInteraction) &&
                     cancellationInteraction != null
                ? cancellationInteraction.GetCancellationToken()
                : new CancellationToken(false);

            var rng = RandomNumberGenerator.Create();
            var handshaker = new Handshaker(
                logger,
                selectedPrivateKeyInteraction.Payload,
                publicKeyStoreInteraction.Payload,
                connectionInteraction.Payload.Protocol,
                rng, TestRemoteIdentifier, localIdentifier);

            var remotePublicKey = handshaker.GetRemotePublicKey(true).RSA;
            var keyExchange = new RsaToAesKeyExchange(selectedPrivateKeyInteraction.Payload, remotePublicKey);
            var transponders =
                new EncryptedTransponderFactory(logger, keyExchange, connectionInteraction.Payload.Protocol, rng);

            using (var active = transponders.CreateTransponder())
            {
                active.Start();
                OnThen?.Invoke(this, new DuplexInteraction(interaction, active.Stream));
            }

            bool TestRemoteIdentifier(string arg)
            {
                var ri = new RepeatInteraction(repeatName, interaction, ct) { IsRunning = false };
                OnNameOffer?.Invoke(this, ri);
                return ri.IsRunning;
            }
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}