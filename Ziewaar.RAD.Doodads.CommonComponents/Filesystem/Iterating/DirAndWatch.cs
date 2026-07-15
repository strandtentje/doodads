#pragma warning disable 67
using System.Collections.Concurrent;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Iterating;

[Category("System & IO")]
[Title("Produces a list of directories, given the path currently in the Register, and watch for changes")]
[Description("""
             Dir will query filesystem for the directory path provided in the Register.
             The events are intended for finding out if the Directory exists, what its 
             subdirectories are, and what its files are. Dirs come out at OnThen, Files come out at OnElse.
             Will also output contents that change.
             """)]
public class DirAndWatch : Dir
{
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        var baseline = base.GetItems(constants, repeater);
        using (var be = baseline.GetEnumerator())
        {
            while (be.MoveNext())
                yield return be.Current;
        }

        var gdi = GetDirectoryInfo(constants, repeater, out var dsi, out var fsi);

        using (var fsw = new FileSystemWatcher(gdi.FullName, dsi))
        {
            BlockingCollection<DirectoryInfo> bc = new BlockingCollection<DirectoryInfo>();


            void ChangeToPropagate(object sender, FileSystemEventArgs e)
            {
                if (!Directory.Exists(e.FullPath))
                    return;
                var di = new DirectoryInfo(e.FullPath);
                bc.Add(di);
            }

            fsw.IncludeSubdirectories = false;
            fsw.Created += ChangeToPropagate;
            fsw.Renamed += ChangeToPropagate;
            fsw.EnableRaisingEvents = true;
            try
            {
                while (fsw.EnableRaisingEvents &&
                    bc.TryTakeResillientBlocking(_ => { }, out var di) == BlockingCollectionExtensions.BlockingTakeResult.ItemSuccess &&
                    di != null)
                    yield return repeater.AppendRegister(di);
            }
            finally
            {
                fsw.EnableRaisingEvents = false;
                fsw.Created -= ChangeToPropagate;
                fsw.Renamed -= ChangeToPropagate;
            }
        }
    }

    protected override IEnumerable<IInteraction> GetElseItems(StampedMap constants, IInteraction repeater)
    {
        var baseline = base.GetElseItems(constants, repeater);
        using (var be = baseline.GetEnumerator())
        {
            while (be.MoveNext())
                yield return be.Current;
        }

        var gdi = GetDirectoryInfo(constants, repeater, out var dsi, out var fsi);

        using (var fsw = new FileSystemWatcher(gdi.FullName, dsi))
        {
            BlockingCollection<FileInfo> bc = new BlockingCollection<FileInfo>();


            void ChangeToPropagate(object sender, FileSystemEventArgs e)
            {
                if (!File.Exists(e.FullPath))
                    return;
                var di = new FileInfo(e.FullPath);
                bc.Add(di);
            }

            fsw.IncludeSubdirectories = false;
            fsw.Created += ChangeToPropagate;
            fsw.Renamed += ChangeToPropagate;
            fsw.EnableRaisingEvents = true;
            try
            {
                while (fsw.EnableRaisingEvents &&
                    bc.TryTakeResillientBlocking(_ => { }, out var di) == BlockingCollectionExtensions.BlockingTakeResult.ItemSuccess &&
                    di != null)
                    yield return repeater.AppendRegister(di);
            }
            finally
            {
                fsw.EnableRaisingEvents = false;
                fsw.Created -= ChangeToPropagate;
                fsw.Renamed -= ChangeToPropagate;
            }
        }
    }
}