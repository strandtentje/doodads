namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Server.Support
{
    public class SshServerInteraction(IInteraction interaction, SshServer server) : IInteraction
    {
        public IInteraction Stack => interaction;
        public object Register => interaction.Register;
        public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
        public SshServer SshServer => server;
    }
}