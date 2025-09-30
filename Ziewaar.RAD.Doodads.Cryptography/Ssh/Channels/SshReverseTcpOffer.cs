#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Channels;
public class SshReverseTcpOffer : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnAlternativePort;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<SshSessionInteraction>(out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH session interaction required"));
            return;
        }

        sessionInteraction.Session.Request += (_, args) =>
        {
            if (args.Request is not SessionRequestMessage requestMessage)
                return;
            if (args.RequestType != "tcpip-forward")
                return;

            var portForwardRequestMessage = requestMessage.ConvertTo<PortForwardRequestMessage>();

            var claimsSourcingInteraction = new ClaimsSourcingInteraction(interaction, args.Principal);

            var tsi = new TextSinkingInteraction(claimsSourcingInteraction);
            OnAlternativePort?.Invoke(this, tsi);
            var alternativePortText = tsi.ReadAllText();
            uint definitivePort = portForwardRequestMessage.Port;
            if (!string.IsNullOrWhiteSpace(alternativePortText) &&
                ushort.TryParse(alternativePortText, out var alternatvePortValue))
                definitivePort = alternatvePortValue;

            if (args.Request.WantReply)
                args.ResponseTask = Task.FromResult<SshMessage>(new SshPfwOkMessage(definitivePort));

            OnThen?.Invoke(this, new CommonInteraction(claimsSourcingInteraction, new SwitchingDictionary(
                ["tcpip-forward-ip", "tcpip-forward-port", "definitive-port"], key => key switch
                {
                    "tcpip-forward-ip" => portForwardRequestMessage.AddressToBind ?? "",
                    "tcpip-forward-port" => portForwardRequestMessage.Port,
                    "definitive-port" => definitivePort,
                    _ => throw new KeyNotFoundException(),
                })));
        };
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}