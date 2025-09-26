using Microsoft.DevTunnels.Ssh;
using Microsoft.DevTunnels.Ssh.Events;

namespace Ziewaar.RAD.Doodads.Cryptography;
public class SetSshSessionShelved : IService
{
    internal static readonly SortedList<Guid, SshServerSession> ShelvedSessions = new();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<SshSessionInteraction>(out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH session interaction required"));
            return;
        }

        var shelfGuid = Guid.NewGuid();

        void RemoveShelvedSession(object? sender, SshSessionClosedEventArgs args)
        {
            sessionInteraction.Session.Closed -= RemoveShelvedSession;
            ShelvedSessions.Remove(shelfGuid);
        }

        sessionInteraction.Session.Closed += RemoveShelvedSession;
        if (sessionInteraction.Session.IsClosed)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Cannot shelve closed sessions"));
            sessionInteraction.Session.Closed -= RemoveShelvedSession;
            return;
        }
        else
        {
            ShelvedSessions.Add(shelfGuid, sessionInteraction.Session);
            var claimsInteraction = new ClaimsSourcingInteraction(interaction, sessionInteraction.Session.Principal);
            OnThen?.Invoke(this, new CommonInteraction(claimsInteraction, register: shelfGuid.ToString()));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}