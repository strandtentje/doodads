#pragma warning disable CS0162 // Unreachable code detected
using Ziewaar.RAD.Doodads.RKOP.Text;
namespace Ziewaar.RAD.Doodads.ModuleLoader;
public class KnownProgram : IDisposable
{
    public ServiceExpression<ServiceBuilder> DescriptionRoot;
    public DirectoryInfo DirectoryInfo;
    public FileInfo FileInfo;
    private bool CurrentlyReloading = false;
    private readonly object FileRefreshLock = new();

    private ServiceBuilder serviceBuilder => (ServiceBuilder)(IInstanceWrapper)DescriptionRoot.ResultSink;
    public IEntryPoint EntryPoint => serviceBuilder;
    public void Dispose()
    {
        DescriptionRoot.UpdateFrom("", ref CursorText.Empty);
        serviceBuilder.Cleanup();
    }
    public void Reload(bool force = false, int attempts = 0)
    {
        if (CurrentlyReloading)
            return;
        lock (FileRefreshLock)
        {
            this.CurrentlyReloading = true;
            try
            {
                Console.WriteLine("Reloading program {0}", FileInfo.Name);
                var cursor = CursorText.Create(FileInfo.Directory, FileInfo.Name, File.ReadAllText(FileInfo.FullName));
                DescriptionRoot.UpdateFrom(FileInfo.Name, ref cursor);
            }
            catch (IOException)
            {
                if (attempts < 6)
                {
                    Console.WriteLine("file still in use. postponing reload job attempt {0}", attempts);
                    var x = new Timer(new TimerCallback(_ =>
                    {
                        Reload(attempts: attempts + 1);
                    }), null, 2500, Timeout.Infinite);
                }
                else
                {
                    Console.WriteLine("failed to reload file after {0} attempts.", attempts);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                throw;
#endif
                Console.WriteLine(ex);
            }
            finally
            {
                this.CurrentlyReloading = false;
                GC.Collect();
            }
        }
    }
    public void Save(int attempts = 0)
    {
        lock (FileRefreshLock)
        {
            try
            {
                Console.WriteLine("Saving program {0}", FileInfo.FullName);
                using (var writer = new StreamWriter(FileInfo.FullName))
                {
                    DescriptionRoot.WriteTo(writer);
                }
            }
            catch (IOException)
            {
                if (attempts < 6)
                {
                    Console.WriteLine("file in use, can't save. planning attempt {0}", attempts);
                    var x = new Timer(new TimerCallback(_ =>
                    {
                        Save(attempts + 1);
                    }), null, 2500, Timeout.Infinite);
                }
                else
                {
                    Console.WriteLine("failed to save after {0} attempts", attempts);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                throw;
#endif
                Console.WriteLine(ex);
            }
            finally
            {
                GC.Collect();
            }
        }
        Reload(force: true);
    }
}
