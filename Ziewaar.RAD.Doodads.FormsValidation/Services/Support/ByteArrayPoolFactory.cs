using System.Collections.Concurrent;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support
{
    public sealed class ByteArrayPoolFactory
    {
        public static readonly ByteArrayPoolFactory Instance = new(); 
        private readonly ConcurrentDictionary<PoolKey, ByteArrayPool> _pools = new();

        public ByteArrayPool GetOrCreate(int arrayCount, int arraySize, TimeSpan timeout)
        {
            var key = new PoolKey(arrayCount, arraySize, timeout);
            return _pools.GetOrAdd(key, _ => new ByteArrayPool(arrayCount, arraySize, timeout));
        }

        private readonly record struct PoolKey(int Count, int Size, TimeSpan Timeout);
    }
}