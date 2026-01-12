using Ziewaar.Network.Protocol;

namespace Ziewaar.RAD.Networking;

public struct InteractionChannelMessage : IMessageWithPayload
{
    public InteractionOperation Operation;
    public int NextLength;
}