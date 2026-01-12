using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Ziewaar.Network.Protocol;

namespace Ziewaar.RAD.Networking;

public class SideChannelDictionary(ProtocolOverStream interactionProtocol) : IReadOnlyDictionary<string, object>
{
    private readonly Lock ProtocolLock = new();

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => 0;
    public bool ContainsKey(string key) => TryGetValue(key, out var _);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
    {
        lock (ProtocolLock)
        {
            var utf8Key = Encoding.UTF8.GetBytes(key);
            interactionProtocol.SendMessage(new InteractionChannelMessage()
            {
                NextLength = utf8Key.Length, Operation = InteractionOperation.ValueRequest
            });
            interactionProtocol.SendMessage(new InteractionChannelMessage() { Operation = InteractionOperation.Name },
                utf8Key);
            var response = interactionProtocol.ReceiveMessage<InteractionChannelMessage>();
            if (response.Operation != InteractionOperation.StringResponse)
                throw new ProtocolViolationException("Protocol breakdown; expected string response");
            if (response.NextLength < 0)
            {
                value = null;
                return false;
            }

            byte[] valueAlloc = new byte[response.NextLength];

            var valueResponse = interactionProtocol.ReceiveMessage<InteractionChannelMessage>(valueAlloc);
            if (valueResponse.Operation != InteractionOperation.Value)
                throw new ProtocolViolationException("Protocol breakdown; expected value response");

            value = Encoding.UTF8.GetString(valueAlloc);
            return true;
        }
    }

    public object this[string key] => TryGetValue(key, out var val) ? val : throw new KeyNotFoundException();

    public IEnumerable<string> Keys { get; } = [];
    public IEnumerable<object> Values { get; } = [];
}