namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions;

[Category("Networking & Connections")]
[Title("Open SSH Session Channel")]
[Description("This is a placeholder and currently doesn't work.")]
public class SshSessionChannel : IService
{
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        /*TODO*/
    }
    private void HandleChannel(SshServer server)
    {
    }
    public void HandleFatal(IInteraction source, Exception ex)
    {
    }
}