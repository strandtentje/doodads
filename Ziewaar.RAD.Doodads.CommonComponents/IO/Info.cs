#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.IO
{
    public class Info : IService
    {
        public event CallForInteraction? OnThen;
        public event CallForInteraction? OnElse;
        public event CallForInteraction? OnException;

        public void Enter(StampedMap constants, IInteraction interaction)
        {
            if (interaction.Register is FileSystemInfo info)
            {
                var payload = new SortedList<string, object>() {
                    { "path", info.FullName },
                    { "name", info.Name },
                    { "write", info.LastWriteTimeUtc },
                    { "read", info.LastAccessTimeUtc },
                };
                if (info.Exists)
                    OnThen?.Invoke(this, new CommonInteraction(interaction, memory: payload));
                else
                    OnElse?.Invoke(this, new CommonInteraction(interaction, memory: payload));
            } else
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "this is not a file or directory"));
            }
        }
        public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    }
}
