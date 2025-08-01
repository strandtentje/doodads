namespace Ziewaar.RAD.Doodads.CoreLibrary;

public class SingletonResourceRepository<TIdent, TInstance> : IDisposable where TInstance : IDisposable where TIdent : IComparable
{
    private bool _disposed;
    private object _instanceLock = new();
    private static SingletonResourceRepository<TIdent, TInstance> _instance;
    public static SingletonResourceRepository<TIdent, TInstance> Get() => _instance ??= new SingletonResourceRepository<TIdent, TInstance>();
    private readonly SortedList<TIdent, (TInstance Instance, SortedSet<Guid> Users)> instanceUsers = new();
    public (Guid Guid, TInstance Instance) Take(TIdent ident, Func<TIdent, TInstance> factory)
    {
        lock (_instanceLock)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SingletonResourceRepository<TIdent, TInstance>));
            var guid = Guid.NewGuid();
            if (!instanceUsers.TryGetValue(ident, out var candidateInstanceUsers))
                candidateInstanceUsers = instanceUsers[ident] = (factory(ident), new());
            candidateInstanceUsers.Users.Add(guid);
            return (guid, candidateInstanceUsers.Instance);
        }
    }
    public void Return(TIdent ident, Guid guid)
    {
        lock (_instanceLock)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SingletonResourceRepository<TIdent, TInstance>));
            if (instanceUsers.TryGetValue(ident, out var candidateInstanceUsers))
            {
                candidateInstanceUsers.Users.Remove(guid);
                if (candidateInstanceUsers.Users.Count == 0)
                {
                    candidateInstanceUsers.Instance.Dispose();
                    instanceUsers.Remove(ident);
                }
            }
        }
    }
    public void Dispose()
    {
        lock (_instanceLock)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SingletonResourceRepository<TIdent, TInstance>));
            _disposed = true;
            foreach (var item in instanceUsers)
            {
                item.Value.Instance.Dispose();
            }
        }
    }
}
