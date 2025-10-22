#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Channels;
[Category("Networking & Connections")]
[Title("Await TCP host offer")]
[Description("""
             Anticipate a TCP server via the connected SSH Client 
             """)]
public class SshReverseTcpOffer : IService
{
    [EventOccasion("""
                    When a server port comes in via the SSH client, It'll come out here with
                    tcpipforwardip; the ip on which the client would like us to listen, 
                    tcpipforwardport; the port on which the client would like us to listen, 
                    definitiveport; the port at which we're ultimately listening. 
                    """)]
    public event CallForInteraction? OnThen;
    [EventOccasion("""
                   If we must listen on a different port and we want to let the client know, 
                   sink that port here.
                   """)]
    public event CallForInteraction? OnAlternativePort;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When there's no ssh session whose tcp offers we can ingest")]
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
                ["tcpipforwardip", "tcpipforwardport", "definitiveport"], key => key switch
                {
                    "tcpipforwardip" => portForwardRequestMessage.AddressToBind ?? "",
                    "tcpipforwardport" => portForwardRequestMessage.Port,
                    "definitiveport" => definitivePort,
                    _ => throw new KeyNotFoundException(),
                })));
        };
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}