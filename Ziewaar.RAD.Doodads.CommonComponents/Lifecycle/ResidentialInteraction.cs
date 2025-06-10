using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Lifecycle;

public class ResidentialInteraction : IInteraction, IDisposable
{
    private readonly SemaphoreSlim Blocker;
    private bool IsDisposed;
    public string Name { get; }
    private ResidentialInteraction(IInteraction parent, string name, SemaphoreSlim blocker)
    {
        this.Stack = parent;
        this.Name = name;
        this.Blocker = blocker;
    }
    public static ResidentialInteraction CreateBlocked(IInteraction parent, string name) => new ResidentialInteraction(parent, name, new SemaphoreSlim(0, 1));
    public void Dispose()
    {
        if (!this.IsDisposed)
        {
            this.IsDisposed = true;
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
        return !IsDisposed;
    }

    public void Leave() => Blocker.Release();
    public IInteraction Stack { get; }
    public object Register => Stack.Register;
    public IReadOnlyDictionary<string, object> Memory => Stack.Memory;
}
