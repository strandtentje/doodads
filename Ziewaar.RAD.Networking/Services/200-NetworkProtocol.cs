using Serilog;
using Ziewaar.Network.Memory;
using Ziewaar.Network.Protocol;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Networking;

public class NetworkProtocol : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var namedItems = constants.NamedItems.ToArray();
        if (namedItems.Length != 1 ||
            namedItems[0].Key is not string protocolName ||
            namedItems[0].Value is not decimal protocolVersion ||
            string.IsNullOrWhiteSpace(protocolName) ||
            protocolName.Length > 16 ||
            protocolVersion < 1)
        {
            OnException?.Invoke(this, interaction.AppendRegister("""
                                                                 Expecting one constants pair protocol name="protocol version"
                                                                 protocol name must be a string shorter than 16
                                                                 protocol version must be a number greater than 0
                                                                 """));
        }
        else if (GlobalLog.Instance is not ILogger logger)
        {
            OnException?.Invoke(this, interaction.AppendRegister("""
                                                                 Network protcols may only be defined in runtimes that have
                                                                 a logger.
                                                                 """));
        }
        else
        {
            var protocolFactory = new TcpBasedProtocolFactory(logger,
                new ProtocolOverStreamFactory(logger, new StructMemoryPool(), new MessageTypeNames(),
                    new ProtocolDefinition(protocolName, (int)protocolVersion)));
            OnThen?.Invoke(this, new CustomInteraction<TcpBasedProtocolFactory>(interaction, protocolFactory));
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}