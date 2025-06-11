using System;
using System.Collections.Generic;
using System.Text;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Reflection
{
    public class ProgDirs : IService
    {
        public event CallForInteraction OnThen;
        public event CallForInteraction OnElse;
        public event CallForInteraction OnException;

        public void Enter(StampedMap constants, IInteraction interaction)
        {
            var allDirs = ProgramRepository.Instance.
                GetKnownPrograms().
                Select(x => (x.DirectoryInfo.FullName, x.DirectoryInfo));
            var sortedDistinctDirs = allDirs.
                Distinct(new FullNameComparer()).
                OrderBy(x => x.FullName);

            List<(string FullName, DirectoryInfo DirectoryInfo)> rootDirectories = new();

            foreach (var item in sortedDistinctDirs)
                if (rootDirectories.Count == 0 || !item.FullName.StartsWith(rootDirectories.Last().FullName))
                    rootDirectories.Add(item);

            OnThen?.Invoke(this, new CommonInteraction(interaction, rootDirectories.Select(x => x.DirectoryInfo).ToArray()));
        }
        public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    }
}
