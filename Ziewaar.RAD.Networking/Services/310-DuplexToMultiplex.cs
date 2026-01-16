using Nerdbank.Streams;
using Serilog;
using System.Text;
using Ziewaar.Network.Protocol;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Networking;

public class DuplexToMultiplex : IService, IDisposable
{
    private readonly HashSet<(MultiplexingStream Multiplex, CancellationTokenSource CTS)> OpenMultiplexers = new();
    private readonly UpdatingPrimaryValue SideChannelNameConstant = new();

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<DuplexInteraction>(out var duplexInteraction) || duplexInteraction == null)
            throw new Exception("Multiplex for now only works on duplex stream source like encrypteduplex");
        else if (constants.PrimaryConstant.ToString() is not { } localSidechannelName ||
                 string.IsNullOrWhiteSpace(localSidechannelName) || localSidechannelName.Length > 128)
            throw new Exception("Specify side channel name different from the other end with max 128 chars");
        else if (GlobalLog.Instance is not ILogger logger)
            throw new Exception("Runtime with logger required for this");
        else
        {
            using var cts = new CancellationTokenSource();
            var ct = cts.Token;

            byte[] singleLengthByte = new byte[10];
            var localSideChannelAscii = Encoding.ASCII.GetBytes(localSidechannelName);
            var duplex = duplexInteraction.DuplexStream;
            singleLengthByte[0] = (byte)localSideChannelAscii.Length;
            duplex.Write(singleLengthByte, 0, 10);
            duplex.Write(localSideChannelAscii, 0, localSideChannelAscii.Length);

            duplex.Flush();

            duplex.ReadExactly(singleLengthByte, 0, 10);
            byte[] remoteSideChannelAscii = new byte[singleLengthByte[0]];
            duplex.ReadExactly(remoteSideChannelAscii, 0, singleLengthByte[0]);
            var remoteSidechannelName = Encoding.ASCII.GetString(remoteSideChannelAscii);

            var multiplexing = MultiplexingStream.Create(duplexInteraction.DuplexStream,
                new MultiplexingStream.Options() { ProtocolMajorVersion = 3, StartSuspended = true, });
            multiplexing.StartListening();
            OpenMultiplexers.Add((multiplexing, cts));
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
                                interaction, (multiplexing, localSidechannel, localProtocol, new())));
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
                
                Thread.Sleep(333); // hmm

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
                    return;
                }


                while (true)
                {
                    try
                    {
                        var received = remoteProtocol.ReceiveMessage<SideChannelMessage, object>(x => x, ct);

                        if (received.message.Operation == SideChannelOperation.Terminate)
                            break;
                        if (received.message.Operation != SideChannelOperation.Initiate)
                            throw new Exception("Unstable multiplex state; wrong message or channel already present");

                        var interactionChannel = multiplexing.AcceptChannelAsync($"i{received.message.Guid.ToString()}", ct).Result;
                        var dataChannel = multiplexing.AcceptChannelAsync($"d{received.message.Guid.ToString()}", ct).Result;

                        var interactionProtocol = MultiplexProtocolFactories.InteractionChannel.Create(interactionChannel.AsStream());
                        var dataStream = dataChannel.AsStream();

                        Task.Run(() =>
                            OnThenAndKeepalive(constants, interaction, ct, interactionChannel, interactionProtocol,
                                dataChannel, dataStream, cts));
                    }
                    catch (Exception ex)
                    {
                        GlobalLog.Instance?.Warning(ex, "While trying to open incoming channel");
                        break;
                    }
                }

                GlobalLog.Instance?.Information(
                    "Remote says there'll be nothing on the sidechannel anymore. Waiting for local sidechanneling to complete.");
                channelOpeningThread.Join();
                GlobalLog.Instance?.Information("Local sidechanneling stopped.");
            }
            finally
            {
                OpenMultiplexers.Remove((multiplexing, cts));
                multiplexing.DisposeAsync();
            }
        }
    }

    private void OnThenAndKeepalive(StampedMap constants, IInteraction interaction, CancellationToken ct,
        MultiplexingStream.Channel interactionChannel, ProtocolOverStream interactionProtocol,
        MultiplexingStream.Channel dataChannel, Stream dataStream, CancellationTokenSource cts)
    {
        var ri = new RepeatInteraction(
            constants.PrimaryConstant.ToString() ?? throw new Exception("repeat name required"),
            interaction, ct);
        ri.IsRunning = false;
        using (interactionChannel)
        using (dataChannel)
        {
            Lock protocolLock = new();
            EventWaitHandle working = new(false, EventResetMode.ManualReset);
            var keepaliveThread = new Thread(() =>
            {
                using (working)
                {
                    try
                    {
                        while (!working.WaitOne(1000) && !ct.IsCancellationRequested)
                        {
                            lock (protocolLock)
                            {
                                interactionProtocol.SendMessage(
                                    new InteractionChannelMessage()
                                    {
                                        NextLength = 1, Operation = InteractionOperation.Syn,
                                    }, cancellationToken: ct);
                            }
                        }

                        lock (protocolLock)
                        {
                            interactionProtocol.SendMessage(
                                new InteractionChannelMessage()
                                {
                                    NextLength = 0, Operation = InteractionOperation.Syn,
                                }, cancellationToken: ct);
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalLog.Instance?.Warning(ex, "while signalling keepalive");
                    }
                }
            });
            keepaliveThread.Start();
            try
            {
                OnThen?.Invoke(this,
                    new MultiplexedInteraction(ri, interactionProtocol, dataStream, protocolLock));
            }
            finally
            {
                try
                {
                    working.Set();
                    keepaliveThread.Join();
                    if (!ri.IsRunning) cts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // whatever
                }
                catch (Exception ex)
                {
                    GlobalLog.Instance?.Warning(ex, "when signalling breakdown");
                }
            }
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);

    public void Dispose()
    {
        foreach (var pair in OpenMultiplexers)
        {
            try
            {
                using (pair.CTS)
                using (pair.Multiplex)
                    pair.CTS.Cancel();
            }
            catch (Exception ex)
            {
                GlobalLog.Instance?.Warning(ex, "While trying to dispose {type} of {service}",
                    nameof(MultiplexingStream), nameof(DuplexToMultiplex));
            }
        }
    }
}