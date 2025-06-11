#nullable enable
#pragma warning disable 67
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.IO
{
    public class Dir : IService
    {
        private readonly UpdatingPrimaryValue FileSearchPatternConstant = new();
        private readonly UpdatingKeyValue DirSearchPattern = new("filterdirs");
        public event CallForInteraction? OnThen;
        public event CallForInteraction? OnElse;
        public event CallForInteraction? OnException;
        public void Enter(StampedMap constants, IInteraction interaction)
        {
            (constants, FileSearchPatternConstant).IsRereadRequired(() => "*", out var fileSearchPattern);
            (constants, DirSearchPattern).IsRereadRequired(() => "*", out var dirSearchPattern);
            fileSearchPattern ??= "*";
            dirSearchPattern ??= "*";

            var dirPath = interaction.Register.ToString();
            DirectoryInfo info = new DirectoryInfo(dirPath);

            if (!info.Exists)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "directory not found"));
                return;
            }

            if (OnThen != null)
            OnThen.Invoke(this, new CommonInteraction(interaction, info.GetDirectories(dirSearchPattern, SearchOption.TopDirectoryOnly)));
            if (OnElse != null)
            OnElse?.Invoke(this, new CommonInteraction(interaction, info.GetFiles(fileSearchPattern, SearchOption.TopDirectoryOnly)));
        }
        public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    }
}
