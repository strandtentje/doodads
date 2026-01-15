using System.Collections.Concurrent;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

public static class BlockingCollectionExtensions
{
    public enum BlockingTakeResult { Complete, ItemSuccess, ListFailure, ItemFailure, }
    extension<TOutput>(BlockingCollection<TOutput> collection)
    {
        public BlockingTakeResult TryTakeResillientBlocking(Action<TOutput> callback, out TOutput? item)
        {
            item = default;
            try
            {
                item = collection.Take();
                callback(item);
                return BlockingTakeResult.ItemSuccess;
            }
            catch (ObjectDisposedException)
            {
                return BlockingTakeResult.Complete;
            }
            catch (InvalidOperationException)
            {
                return collection.IsCompleted ? BlockingTakeResult.Complete : BlockingTakeResult.ListFailure;
            }
            catch (Exception ex)
            {
                GlobalLog.Instance?.Warning(ex, "While taking item of {type}", typeof(TOutput).Name);
                return BlockingTakeResult.ItemFailure;
            }
        }
    }
}
