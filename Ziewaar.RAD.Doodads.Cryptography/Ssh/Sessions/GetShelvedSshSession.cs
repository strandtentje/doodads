namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions;

[Category("Networking & Connections")]
[Title("Get Shelved SSH Session")]
[Description("""
             Provided a GUID in the register, retrieves the SSH session 
             """)]
public class GetShelvedSshSession : IService
{
    [EventOccasion("When the session was alive on the shelf, it comes out here.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the session died on the shelf, it comes out here.")]
    public event CallForInteraction? OnClosed;
    [EventOccasion("When there was no such session on the shelf, this happens.")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when there was no sane GUID in register.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        string? guidString = interaction.Register.ToString();
        if (string.IsNullOrWhiteSpace(guidString) || 
            !Guid.TryParse(guidString, out var shelvedGuid))
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "guid required in register to get shelved ssh session"));
            return;
        }

        if (SetSshSessionShelved.ShelvedSessions.TryGetValue(shelvedGuid,
                out var session))
        {
            if (session.IsClosed || !session.IsConnected)
            {
                var claimsInteraction =
                    new ClaimsSourcingInteraction(interaction,
                        session.Principal);
                OnClosed?.Invoke(this,
                    new SshSessionInteraction(claimsInteraction, session));
            }
            else
            {
                var claimsInteraction =
                    new ClaimsSourcingInteraction(interaction,
                        session.Principal);
                OnThen?.Invoke(this,
                    new SshSessionInteraction(claimsInteraction, session));
            }
        }
        else
            OnElse?.Invoke(this, interaction);
    }

    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}