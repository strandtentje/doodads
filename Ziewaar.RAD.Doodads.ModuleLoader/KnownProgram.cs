using System;
using System.IO;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.RKOP;

namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class KnownProgram : IDisposable
{
    public event EventHandler RequiresSaving;

    public ServiceDescription<ServiceBuilder> DescriptionRoot;
    public DirectoryInfo DirectoryInfo;
    public FileInfo FileInfo;
    private ServiceBuilder _serviceBuilder => (ServiceBuilder)(IInstanceWrapper)DescriptionRoot.Wrapper;
    public IEntryPoint EntryPoint => _serviceBuilder;

    public void Dispose()
    {
        DescriptionRoot.UpdateFrom(ref CursorText.Empty);
        _serviceBuilder.Cleanup();
    }
}
