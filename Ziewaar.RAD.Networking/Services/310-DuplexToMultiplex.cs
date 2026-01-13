using Nerdbank.Streams;
using Serilog;
using System.Text;
using Ziewaar.Network.Protocol;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Networking;

public class DuplexToMultiplex : IteratingService, IDisposable
{
    private readonly HashSet<MultiplexingStream> OpenMultiplexers = new();
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
            OpenMultiplexers.Add(multiplexing);
            var localSidechannelOffer = multiplexing.OfferChannelAsync(localSidechannelName, ct);

            var channelOpeningThread = new Thread(() =>
            {
                try
                {
                    using var localSidechannel = localSidechannelOffer.Result;
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
                                new SideChannelMessage()
                                {
                                    Guid = Guid.Empty, Operation = SideChannelOperation.Terminate
                                },
                                cancellationToken: ct);
                        }
                        catch (Exception ex)
                        {
                            GlobalLog.Instance?.Warning(ex, "Unable to close gracefully");
                        }
                    }
                }
                catch (Exception ex)
                {
                    GlobalLog.Instance?.Warning(ex, "While awaiting more channel opens");
                }
            });

            try
            {
                channelOpeningThread.Start();

                MultiplexingStream.Channel? remoteSidechannel = null;
                ProtocolOverStream? remoteProtocol = null;

                try
                {
                    remoteSidechannel = multiplexing.AcceptChannelAsync(remoteSidechannelName, ct).Result;
                    remoteProtocol = MultiplexProtocolFactories.SideChannel.Create(remoteSidechannel.AsStream(true));
                }
                catch (Exception ex)
                {
                    GlobalLog.Instance?.Warning(ex, "While trying to open incoming sidechannel");
                }

                if (remoteSidechannel == null || remoteProtocol == null)
                {
                    GlobalLog.Instance?.Information("Sidechannel and protocol setup failed; not receiving those.");
                    yield break;
                }

                while (true)
                {
                    ProtocolOverStream? interactionProtocol = null;
                    Stream? dataStream = null;

                    try
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

                        interactionProtocol =
                            MultiplexProtocolFactories.InteractionChannel.Create(interactionChannel.AsStream());
                        dataStream = dataChannel.AsStream();
                    }
                    catch (Exception ex)
                    {
                        GlobalLog.Instance?.Warning(ex, "While trying to open incoming channel");
                    }

                    if (interactionProtocol == null || dataStream == null)
                    {
                        GlobalLog.Instance?.Information("Channel open failed; not continuing");
                        yield break;
                    }

                    yield return new MultiplexedInteraction(repeater, interactionProtocol, dataStream);
                }

                GlobalLog.Instance?.Information(
                    "Remote says there'll be nothing on the sidechannel anymore. Waiting for local sidechanneling to complete.");
                channelOpeningThread.Join();
                GlobalLog.Instance?.Information("Local sidechanneling stopped.");
            }
            finally
            {
                OpenMultiplexers.Remove(multiplexing);
                multiplexing.DisposeAsync();
            }
        }
    }

    public void Dispose()
    {
        foreach (MultiplexingStream multiplexingStream in OpenMultiplexers)
        {
            try
            {
                multiplexingStream.DisposeAsync();
            }
            catch (Exception ex)
            {
                GlobalLog.Instance?.Warning(ex, "While trying to dispose {type} of {service}", nameof(multiplexingStream), nameof(DuplexToMultiplex));
            }
        }
    }
}