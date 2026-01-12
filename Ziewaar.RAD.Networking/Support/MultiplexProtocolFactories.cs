using Ziewaar.Network.Memory;
using Ziewaar.Network.Protocol;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Networking;

public static class MultiplexProtocolFactories
{
    public static readonly StructMemoryPool Memory = new();
    public static readonly MessageTypeNames Names = new();

    public static readonly ProtocolOverStreamFactory SideChannel =
        new ProtocolOverStreamFactory(GlobalLog.Instance!, Memory, Names, new ProtocolDefinition("MPXSC", 3333));

    public static readonly ProtocolOverStreamFactory InteractionChannel =
        new ProtocolOverStreamFactory(GlobalLog.Instance!, Memory, Names, new ProtocolDefinition("MPXIA", 3434));
}