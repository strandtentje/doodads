#nullable enable
#pragma warning disable CS0162 // Unreachable code detected
using Ziewaar.RAD.Doodads.RKOP.Exceptions;
using Ziewaar.RAD.Doodads.RKOP.SeriesParsers;
using Ziewaar.RAD.Doodads.RKOP.Text;
namespace Ziewaar.RAD.Doodads.ModuleLoader;
public class KnownProgram : IDisposable
{
    public delegate void FileReadyHandler();

    public FileReadyHandler? OnReady;
    private UnconditionalSerializableServiceSeries<ServiceBuilder>? RootSeries;
    public DirectoryInfo? DirectoryInfo;
    public FileInfo? FileInfo;
    private bool CurrentlyReloading = false;
    internal IInteraction? AutoStartOnReloadParams;
    private readonly object FileRefreshLock = new();

    private ServiceBuilder? serviceBuilder => (RootSeries?.ResultSink as IInstanceWrapper) as ServiceBuilder;
    public IEntryPoint? EntryPoint => serviceBuilder;
    public void Dispose()
    {        
        serviceBuilder?.Cleanup();
    }
    public void Reload(bool force = false, int attempts = 0)
    {
        if (CurrentlyReloading)
            return;
        lock (FileRefreshLock)
        {
            this.CurrentlyReloading = true;
            if (FileInfo == null)
                throw new ArgumentNullException(nameof(FileInfo));
            try
            {
                if (!FileInfo.Exists)
                {
                    FileInfo.Create();
                    Console.WriteLine("Creating empty file");
                }

                Console.WriteLine("Reloading program {0}", FileInfo.Name);
                var cursor = CursorText.Create(FileInfo.Directory, FileInfo.Name, File.ReadAllText(FileInfo.FullName));

                serviceBuilder?.Cleanup();
                RootSeries = new();
                RootSeries.UpdateFrom(FileInfo.Name, ref cursor);     
                
                if (OnReady != null)
                {
                    OnReady();
                    OnReady = null;
                }    
            }
            catch (IOException iox)
            {
                Console.Write(iox.Message);
                if (attempts < 6)
                {
                    Console.WriteLine("file not accessible. postponing reload job attempt {0}", attempts);
                    var x = new Timer(new TimerCallback(_ =>
                    {
                        Reload(attempts: attempts + 1);
                    }), null, 2500, Timeout.Infinite);
                }
                else
                {
                    Console.WriteLine("failed to reload file after {0} attempts.", attempts);
                }
                return;
            }
            catch(ExceptionAtPositionInFile fex)
            {
                Console.WriteLine(fex.Message);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
#if DEBUG
                throw;
#endif
                return;
            }
            finally
            {
                this.CurrentlyReloading = false;
                GC.Collect();
            }
            if (EntryPoint == null)
            {
                Console.WriteLine($"File {FileInfo} had no entrypoint. Some things may not start.");
                return;
            }
            if (AutoStartOnReloadParams != null)
            {
                EntryPoint.Run(this, AutoStartOnReloadParams);
            }
        }
    }
    public void Save(int attempts = 0)
    {
        lock (FileRefreshLock)
        {
            if (FileInfo == null)
                throw new NullReferenceException("cannot save if theres no file");
            if (RootSeries == null) 
                throw new NullReferenceException("cant save undefined program");
            try
            {
                Console.WriteLine("Saving program {0}", FileInfo.FullName);
                using (var writer = new StreamWriter(FileInfo.FullName))
                {
                    RootSeries.WriteTo(writer);
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
