using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Doodads.RuntimeForDotnetCore.Bootstrapper;

public class MultipleDisposables : List<IDisposable>, IDisposable
{
    public void Dispose()
    {
        foreach (var item in this)
        {
            try
            {
                item.Dispose();
            } catch(Exception ex)
            {
                GlobalLog.Instance?.Warning(ex, "While disposing {0}", item);
            }
        }
    }
}