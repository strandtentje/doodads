#nullable enable
using Ziewaar.RAD.Doodads.ModuleLoader.Delegates;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Filesystem;
#pragma warning disable CS0162
public class ResilientCursorTextEmitter(FileInfo file)
{
    public readonly SingleWorkerGuarantor WorkingState = new();
    public event CursorTextAvailableHandler? CursorTextAvailable;
    public DirectoryInfo? DirectoryInfo => file.Directory;
    public FileInfo FileInfo => file;
    public long LastReadTime { get; private set; }
    private void LockCatchRetry(Action readCallback, int attemptNumber = 0, int maxAttempts = 6)
    {
        file.Refresh();
        if (!WorkingState.TryDoWorkOrWait() || LastReadTime == file.LastWriteTime.Ticks)
            return;
        try
        {
            if (!file.Exists)
            {
                file.Create().Close();
                Console.WriteLine("Creating empty file");
            }

            readCallback();
            LastReadTime = file.LastWriteTime.Ticks;
            WorkingState.WorkHasCeased();
        }
#if !DEBUG
        catch (Exception ex)
        {
            if (ex is IOException || ex is UnauthorizedAccessException)
            {
                Console.Write(ex.Message);
                if (attemptNumber < maxAttempts)
                {
                    Console.WriteLine("file not accessible. postponing reload job attempt {0}", attemptNumber);
                    var x = new Timer(
                        new TimerCallback(_ => { LockCatchRetry(readCallback, attemptNumber + 1, maxAttempts); }), null,
                        2500,
                        Timeout.Infinite);
                }
                else
                {
                    Console.WriteLine("failed to reload file after {0} attempts.", attemptNumber);
                    WorkingState.WorkHasCeased();
                }
            } else if (ex is ExceptionAtPositionInFile pos)
            {
                Console.WriteLine(ex.Message);
                WorkingState.WorkHasCeased();
            }
            else
            {
                Console.WriteLine(ex);
                WorkingState.WorkHasCeased();
                throw;
            }
        }
#endif
        finally
        {
            GC.Collect();
        }
    }
    public void RequestLoad()
    {
        LockCatchRetry(() =>
        {
            Console.WriteLine("Reloading program {0}", file.Name);
            var cursor = CursorText.Create(this.DirectoryInfo, file.Name, File.ReadAllText(file.FullName));

            CursorTextAvailable?.Invoke(this, cursor);
        });
    }
    public void RequestSave(IEnumerable<ProgramDefinition> definitions)
    {
        LockCatchRetry(() =>
        {
            using (var writer = new StreamWriter(file.FullName))
            {
                foreach (var definition in definitions)
                {
                    definition.CurrentSeries.WriteTo(writer);
                    writer.WriteLine();
                }
            }
        });
    }
}