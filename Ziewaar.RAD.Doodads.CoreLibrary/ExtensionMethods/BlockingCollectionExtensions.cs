using Ejije.Logging;
using System.Collections.Concurrent;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

public static class BlockingCollectionExtensions
{
    public enum BlockingTakeResult { Complete, ItemSuccess, ListFailure, ItemFailure, }
    private static readonly Log
        BlockingCollectionTakeFail = Log.Warn("Got {exception} while taking item of {type} from blocking collection");
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
                try
                {
                    return collection.IsCompleted ? BlockingTakeResult.Complete : BlockingTakeResult.ListFailure;
                }
                catch (ObjectDisposedException)
                {
                    return BlockingTakeResult.Complete;
                }
                catch (Exception)
                {
                    return BlockingTakeResult.ListFailure;
                }
            }
            catch (Exception ex)
            {
                Log.Post(BlockingCollectionTakeFail, ex, typeof(TOutput).Name);
                return BlockingTakeResult.ItemFailure;
            }
        }
    }
}
