namespace Ziewaar.RAD.Doodads.Cryptography;
public class GetShelvedSshSession : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        string? guidString = interaction.Register.ToString();
        if (string.IsNullOrWhiteSpace(guidString))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction,"guid required in register to get shelved ssh session"));
            return;
        }
        var shelvedGuid = Guid.Parse(guidString);
        if (SetSshSessionShelved.ShelvedSessions.TryGetValue(shelvedGuid, out var session))
        {
            var claimsInteraction = new ClaimsSourcingInteraction(interaction, session.Principal);
            OnThen?.Invoke(this, new SshSessionInteraction(claimsInteraction, session));
        }
        else 
            OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}