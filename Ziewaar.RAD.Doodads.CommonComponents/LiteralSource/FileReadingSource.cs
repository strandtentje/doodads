namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

public class FileReadingSource : IService
{
    private const string EnableCache = nameof(EnableCache);
    private const string MaxCacheBytes = nameof(MaxCacheBytes);
    private FileInfo LastFileInfo = null;
    private byte[] LastValidData = null;
    [NamedBranch] public event EventHandler<IInteraction> PathSink;
    [NamedBranch] public event EventHandler<IInteraction> OnIoError;

    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        var pathSource = LastFileInfo == null
            ? new RawStringAlwaysSinkingInteraction(interaction, SidechannelState.Always)
            : new RawStringAlwaysSinkingInteraction(interaction, SidechannelState.StampGreater);
        PathSink?.Invoke(this, pathSource);
        if (LastFileInfo == null && !pathSource.TaggedData.Tag.IsTainted)
        {
            OnIoError?.Invoke(this,
                new VariablesInteraction(interaction,
                    new SortedList<string, object> { { "error", "no path provided" } }));
            return;
        }
        if (pathSource.TaggedData.Tag.IsTainted)
        {
            LastFileInfo = new FileInfo(pathSource.GetFullString());
            var maxCacheBytes = serviceConstants.InsertIgnore(MaxCacheBytes, 1024 * 1024 * 32);
            if (serviceConstants.InsertIgnore(EnableCache, LastFileInfo.Length < maxCacheBytes))
            {
                try
                {
                    LastValidData = File.ReadAllBytes(LastFileInfo.FullName);
                }
                catch (Exception ex)
                {
                    OnIoError?.Invoke(this, new VariablesInteraction(interaction, ex.ToSortedList()));
                }
            }
            else
            {
                LastValidData = null;
            }
        }

        var writer = interaction.ResurfaceStream();
        if (!writer.RequireUpdate(LastFileInfo!.LastWriteTime.Ticks))
            return;
        if (LastValidData != null)
        {
            writer.TaggedData.Data.Write(LastValidData, 0, LastValidData.Length);
        }
        else
        {
            using var fileReader = LastFileInfo.OpenRead();
            fileReader.CopyTo(writer.TaggedData.Data);
        }
    }
}