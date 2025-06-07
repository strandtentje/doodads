using System;
using System.IO;
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.RKOP;

namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class KnownProgram : IDisposable
{
    public event EventHandler RequiresSaving;

    public ServiceDescription<ServiceBuilder> DescriptionRoot;
    public DirectoryInfo DirectoryInfo;
    public FileInfo FileInfo;
    private bool _currentlyReloading = false;
    private readonly object _fileRefreshLock = new();

    private ServiceBuilder _serviceBuilder => (ServiceBuilder)(IInstanceWrapper)DescriptionRoot.ResultSink;
    public IEntryPoint EntryPoint => _serviceBuilder;
    public void Dispose()
    {
        DescriptionRoot.UpdateFrom(ref CursorText.Empty);
        _serviceBuilder.Cleanup();
    }
    public void Reload()
    {
        if (_currentlyReloading)
            return;
        lock(_fileRefreshLock)
        {
            this._currentlyReloading = true;
            try
            {
                Console.WriteLine("Reloading program {0}", FileInfo.Name);
                var cursor = CursorText.Create(FileInfo.Directory, FileInfo.Name, File.ReadAllText(FileInfo.FullName));
                DescriptionRoot.UpdateFrom(ref cursor);
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
                this._currentlyReloading = false;
                GC.Collect();
            }
        }
    }
}
