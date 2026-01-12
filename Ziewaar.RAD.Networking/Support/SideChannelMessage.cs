using Ziewaar.Network.Protocol;

namespace Ziewaar.RAD.Networking;

public struct SideChannelMessage : IMessageWithPayload
{
    public SideChannelOperation Operation;
    public Guid Guid;
}