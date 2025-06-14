#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.IO
{
    public class Info : IService
    {
        private readonly UpdatingKeyValue Hidden = new UpdatingKeyValue("hidden");
        public event CallForInteraction? OnThen;
        public event CallForInteraction? OnElse;
        public event CallForInteraction? OnException;

        public void Enter(StampedMap constants, IInteraction interaction)
        {
            (constants, Hidden).IsRereadRequired(() => false, out bool? showHidden);
            if (interaction.Register is FileSystemInfo info)
            {
                var payload = new SortedList<string, object>() {
                    { "path", info.FullName },
                    { "name", info.Name },
                    { "write", info.LastWriteTimeUtc },
                    { "read", info.LastAccessTimeUtc },
                };
                if (!info.Exists)
                    OnElse?.Invoke(this, new CommonInteraction(interaction, memory: payload));
                else if (showHidden == true)
                    OnThen?.Invoke(this, new CommonInteraction(interaction, memory: payload));
                else if (!info.Attributes.HasFlag(FileAttributes.Hidden) && !info.Name.StartsWith("."))
                    OnThen?.Invoke(this, new CommonInteraction(interaction, memory: payload));
            } else
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "this is not a file or directory"));
            }
        }
        public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    }
}
