namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
#pragma warning disable 67
#nullable enable
[Category("Reflection & Documentation")]
[Title("List all directories that contain running programs")]
[Description("""
             For the currently running doodads instance, this service enumerates 
             all open working directories / common ancestors.
             """)]
public class ProgramDirectories : IteratingService
{
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants,
        IInteraction repeater)
    {
        var allDirs = ProgramRepository.Instance.GetKnownPrograms()
            .Select(x => (x.Emitter.DirectoryInfo!.FullName,
                x.Emitter.DirectoryInfo));
        var sortedDistinctDirs = allDirs.Distinct(new FullNameComparer())
            .OrderBy(x => x.FullName).Select(x => x.FullName);
        List<string> resultDirs = new();

        foreach (string sortedDistinctDir in sortedDistinctDirs)
        {
            if (resultDirs.Count == 0 ||
                !sortedDistinctDir.StartsWith(resultDirs.Last()))
            {
                resultDirs.Add(sortedDistinctDir);
                yield return repeater.AppendRegister(sortedDistinctDir);
            }
        }
    }
}