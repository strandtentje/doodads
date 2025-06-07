using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Lifecycle;

public class ResidentialInteraction : IInteraction, IDisposable
{
    private readonly SemaphoreSlim Blocker;
    private bool _isDisposed;

    public string Name { get; }
    public IInteraction Parent { get; }
    public IReadOnlyDictionary<string, object> Variables => Parent.Variables;
    private ResidentialInteraction(IInteraction parent, string name, SemaphoreSlim blocker)
    {
        this.Parent = parent;
        this.Blocker = blocker;
        this.Name = name;
    }
    public static ResidentialInteraction CreateBlocked(IInteraction parent, string name) => new ResidentialInteraction(parent, name, new SemaphoreSlim(0, 1));
    public void Dispose()
    {
        if (!this._isDisposed)
        {
            this._isDisposed = true;
            try
            {
                Blocker.Release();
            } catch (Exception)
            {

            }
            Blocker.Dispose();
        }
    }
    public bool Enter()
    {
        Blocker.Wait();
        return !_isDisposed;
    }

    public void Leave() => Blocker.Release();
}
