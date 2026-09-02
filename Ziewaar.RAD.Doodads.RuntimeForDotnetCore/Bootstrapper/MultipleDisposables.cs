using Ejije.Logging;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Doodads.RuntimeForDotnetCore.Bootstrapper;

public class MultipleDisposables : List<IDisposable>, IDisposable
{
    private static readonly Log
        DisposeError = Log.Oops("Nested Disposal of {item} caused {exception}");
    public void Dispose()
    {
        foreach (var item in this)
        {
            try
            {
                item.Dispose();
            } catch(Exception ex)
            {
                Log.Post(DisposeError, item, ex);
            }
        }
    }
}