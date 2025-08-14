#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.ModuleLoader.Filesystem;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services
{
    public class NeverReload : IService
    {
        private readonly UpdatingPrimaryValue FilenameConstant = new();
        private string? NextFilename;
        private string? CurrentFilename;
        public event CallForInteraction? OnThen;
        public event CallForInteraction? OnElse;
        public event CallForInteraction? OnException;

        public void Enter(StampedMap constants, IInteraction interaction)
        {
            if ((constants, FilenameConstant).IsRereadRequired(out object? filename))
                this.NextFilename = filename?.ToString();
            if (string.IsNullOrWhiteSpace(this.NextFilename) || this.NextFilename == null)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "filename required"));
                return;
            }

            if (this.CurrentFilename == NextFilename)
                return;

            this.NextFilename = (new FileInfo(this.NextFilename).FullName);
            if (this.CurrentFilename != null)
                ResilientCursorTextEmitter.ReloadLocked.Remove(this.CurrentFilename);
            GlobalLog.Instance?.Information("Switching out filename locks from {a} to {b}", CurrentFilename,
                NextFilename);
            this.CurrentFilename = NextFilename;
            ResilientCursorTextEmitter.ReloadLocked.Add(this.CurrentFilename!);
        }

        public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    }
}