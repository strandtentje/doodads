using Nerdbank.Streams;
using System.Net;
using System.Text;
using Ziewaar.Network.Protocol;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Networking;

public class OpenMultiplexChannel : IService, IDisposable
{
    private readonly
        HashSet<(MultiplexingStream.Channel ChannelA, MultiplexingStream.Channel ChannelB, CancellationTokenSource CTS)>
        OpenChannels = new();

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction
                .TryGetClosest<
                    CustomInteraction<(MultiplexingStream Multiplexer, MultiplexingStream.Channel SideChannel,
                        ProtocolOverStream Protocol, Lock ProtocolLock)>>(out var channelInteraction) ||
            channelInteraction == null)
            OnException?.Invoke(this, interaction.AppendRegister("This may only be used with duplextomultiplex"));
        else if (!interaction.TryGetClosest<ISourcingInteraction>(out var sourcingInteraction) ||
                 !interaction.TryGetClosest<ISinkingInteraction>(out var sinkingInteraction) ||
                 sourcingInteraction == null || sinkingInteraction == null)
            OnException?.Invoke(this, interaction.AppendRegister("Sourcing and sinking interaction required"));
        else
        {
            using var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var (multiplexer, channel, protocol, protocolLock) = channelInteraction.Payload;

            var multiplexGuid = Guid.NewGuid();

            var interactionMultiplexTask = multiplexer.OfferChannelAsync($"i{multiplexGuid.ToString()}", ct);
            var duplexMultiplexTask = multiplexer.OfferChannelAsync($"d{multiplexGuid.ToString()}", ct);

            lock (protocolLock)
            {
                protocol.SendMessage(
                    new SideChannelMessage() { Operation = SideChannelOperation.Initiate, Guid = multiplexGuid },
                    cancellationToken: ct);
            }

            var interactionChannel = interactionMultiplexTask.Result;
            using var duplexChannel = duplexMultiplexTask.Result;

            OpenChannels.Add((interactionChannel, duplexChannel, cts));

            var duplexStream = duplexChannel.AsStream();

            var interactionThread = new Thread(() =>
            {
                try
                {
                    using (interactionChannel)
                    {
                        var valueStream = interactionChannel.AsStream();
                        var valueProtocol = MultiplexProtocolFactories.InteractionChannel.Create(valueStream);
                        while (true)
                        {
                            var controlMessage = valueProtocol.ReceiveMessage<InteractionChannelMessage>(ct);
                            if (controlMessage.Operation != InteractionOperation.ValueRequest)
                                throw new ProtocolViolationException("Unstable channel state; expected value request");
                            byte[] receiveAlloc = new byte[controlMessage.NextLength];
                            var nameMessage = valueProtocol.ReceiveMessage<InteractionChannelMessage>(receiveAlloc, ct);
                            if (nameMessage.Operation != InteractionOperation.Name)
                                throw new ProtocolViolationException("Unstable channel state; expected name of value");
                            var nameString = Encoding.UTF8.GetString(receiveAlloc);

                            if (interaction.TryFindVariable(nameString, out object? variable) &&
                                variable?.ToString() is { } toSend)
                            {
                                var utfBytes = Encoding.UTF8.GetBytes(toSend);
                                valueProtocol.SendMessage(
                                    new InteractionChannelMessage()
                                    {
                                        Operation = InteractionOperation.StringResponse,
                                        NextLength = utfBytes.Length,
                                    }, cancellationToken: ct);
                                valueProtocol.SendMessage(
                                    new InteractionChannelMessage() { Operation = InteractionOperation.Value, },
                                    utfBytes, ct);
                            }
                            else
                            {
                                valueProtocol.SendMessage(
                                    new InteractionChannelMessage()
                                    {
                                        Operation = InteractionOperation.StringResponse, NextLength = -1
                                    }, cancellationToken: ct);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    GlobalLog.Instance?.Warning(ex, "Closed channel due to interaction channel failure");
                }
            });

            var outgoingThread = new Thread(() =>
            {
                try
                {
                    sourcingInteraction.SourceBuffer.CopyTo(duplexStream);
                }
                catch (Exception ex)
                {
                    GlobalLog.Instance?.Warning(ex, "Closed channel due to local->remote copy fail");
                }
            });

            var incomingThread = new Thread(() =>
            {
                try
                {
                    duplexStream.CopyTo(sinkingInteraction.SinkBuffer);
                }
                catch (Exception ex)
                {
                    GlobalLog.Instance?.Warning(ex, "Closed channel due to remote->local copy fail");
                }
            });

            interactionThread.Start();
            outgoingThread.Start();
            incomingThread.Start();

            while (interactionThread.IsAlive && outgoingThread.IsAlive && incomingThread.IsAlive)
            {
                interactionThread.Join(1000);
                outgoingThread.Join(1000);
                incomingThread.Join(1000);
            }

            OpenChannels.Remove((interactionChannel, duplexChannel, cts));
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);

    public void Dispose()
    {
        foreach (var pair in OpenChannels)
        {
            try
            {
                using (pair.CTS)
                using (pair.ChannelA)
                using (pair.ChannelB)
                    pair.CTS.Cancel();
            }
            catch (Exception ex)
            {
                GlobalLog.Instance?.Warning(ex, "While disposing {type} of {service}",
                    nameof(MultiplexingStream.Channel), nameof(OpenMultiplexChannel));
            }
        }
    }
}