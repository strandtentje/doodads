using Nerdbank.Streams;
using Serilog;
using System.Text;
using Ziewaar.Network.Protocol;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Networking;

public class DuplexToMultiplex : IteratingService
{
    private readonly UpdatingPrimaryValue SideChannelNameConstant = new();
    public override event CallForInteraction? OnElse;
    protected override bool RunElse => false;

    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (!repeater.TryGetClosest<DuplexInteraction>(out var duplexInteraction) || duplexInteraction == null)
            throw new Exception("Multiplex for now only works on duplex stream source like encrypteduplex");
        else if (constants.PrimaryConstant.ToString() is not { } localSidechannelName ||
                 string.IsNullOrWhiteSpace(localSidechannelName) || localSidechannelName.Length > 128)
            throw new Exception("Specify side channel name different from the other end with max 128 chars");
        else if (GlobalLog.Instance is not ILogger logger)
            throw new Exception("Runtime with logger required for this");
        else
        {
            CancellationToken ct;
            if (repeater.TryGetClosest<CancellationInteraction>(out var cancellationInteraction) &&
                cancellationInteraction != null)
                ct = cancellationInteraction.GetCancellationToken();
            else
                ct = new CancellationToken(false);


            var localSideChannelAscii = Encoding.ASCII.GetBytes(localSidechannelName);
            var duplex = duplexInteraction.DuplexStream;
            duplex.WriteByte((byte)localSideChannelAscii.Length);
            duplex.Write(localSideChannelAscii, 0, localSideChannelAscii.Length);

            var remoteSidechannelLength = duplex.ReadByte();
            byte[] remoteSideChannelAscii = new byte[remoteSidechannelLength];
            duplex.ReadExactly(remoteSideChannelAscii, 0, remoteSidechannelLength);
            var remoteSidechannelName = Encoding.ASCII.GetString(remoteSideChannelAscii);

            var multiplexing = MultiplexingStream.Create(duplexInteraction.DuplexStream);
            var localSidechannelOffer = multiplexing.OfferChannelAsync(localSidechannelName, ct);

            var channelOpeningThread = new Thread(() =>
            {
                var localSidechannel = localSidechannelOffer.Result;
                var localProtocol = MultiplexProtocolFactories.SideChannel.Create(localSidechannel.AsStream(true));

                try
                {
                    OnElse?.Invoke(this,
                        new CustomInteraction<(MultiplexingStream Multiplexer, MultiplexingStream.Channel
                            SideChannel,
                            ProtocolOverStream Protocol, Lock ProtocolLock)>(
                            repeater, (multiplexing, localSidechannel, localProtocol, new())));
                }
                finally
                {
                    try
                    {
                        localProtocol.SendMessage(
                            new SideChannelMessage() { Guid = Guid.Empty, Operation = SideChannelOperation.Terminate },
                            cancellationToken: ct);
                    }
                    catch (Exception ex)
                    {
                        GlobalLog.Instance?.Warning(ex, "Unable to close gracefully");
                    }
                }
            });

            try
            {
                channelOpeningThread.Start();

                var remoteSidechannel = multiplexing.AcceptChannelAsync(remoteSidechannelName, ct).Result;
                var remoteProtocol = MultiplexProtocolFactories.SideChannel.Create(remoteSidechannel.AsStream(true));

                while (true)
                {
                    var received = remoteProtocol.ReceiveMessage<SideChannelMessage, object>(x => x, ct);

                    if (received.message.Operation == SideChannelOperation.Terminate)
                        break;
                    if (received.message.Operation != SideChannelOperation.Initiate)
                        throw new Exception("Unstable multiplex state; wrong message or channel already present");

                    var interactionChannel =
                        multiplexing.AcceptChannelAsync($"i{received.message.Guid.ToString()}", ct).Result;
                    var dataChannel =
                        multiplexing.AcceptChannelAsync($"d{received.message.Guid.ToString()}", ct).Result;

                    var interactionProtocol =
                        MultiplexProtocolFactories.InteractionChannel.Create(interactionChannel.AsStream());

                    yield return new MultiplexedInteraction(repeater, interactionProtocol, dataChannel.AsStream());
                }

                GlobalLog.Instance?.Information(
                    "Remote says there'll be nothing on the sidechannel anymore. Waiting for local sidechanneling to complete.");
                channelOpeningThread.Join();
                GlobalLog.Instance?.Information("Local sidechanneling stopped.");
            }
            finally
            {
                multiplexing.DisposeAsync();
            }
        }
    }
}