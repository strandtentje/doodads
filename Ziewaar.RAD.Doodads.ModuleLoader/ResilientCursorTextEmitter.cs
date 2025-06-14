#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader;
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
        if (!WorkingState.TryDoWorkOrWait())
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
        catch (IOException iox)
        {
            Console.Write(iox.Message);
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
        }
        catch (ExceptionAtPositionInFile fex)
        {
            Console.WriteLine(fex.Message);
            WorkingState.WorkHasCeased();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            WorkingState.WorkHasCeased();
#if DEBUG
            throw;
#endif
            return;
        }
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