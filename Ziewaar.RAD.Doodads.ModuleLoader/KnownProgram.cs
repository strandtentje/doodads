using Ziewaar.RAD.Doodads.RKOP.Blocks;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class KnownProgram : IDisposable
{
    public event EventHandler RequiresSaving;

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
    public void Reload()
    {
        if (CurrentlyReloading)
            return;
        lock(FileRefreshLock)
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
                Console.WriteLine("file still in use. postponing reload job...");
                var x = new Timer(new TimerCallback(_ =>
                {
                    Reload();
                }), null, 200, Timeout.Infinite);
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
}
