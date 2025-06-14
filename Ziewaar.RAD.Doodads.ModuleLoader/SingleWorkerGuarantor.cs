#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader;
#pragma warning disable CS0162
public class SingleWorkerGuarantor
{
    private readonly object LockObject = new object();
    private readonly EventWaitHandle Ewh = new(false, EventResetMode.ManualReset);
    private bool IsWorking = false;
    public bool TryDoWorkOrWait()
    {
        lock (LockObject)
        {
            if (IsWorking)
            {
                Ewh.WaitOne();
                return false;
            }
            else
            {
                Ewh.Reset();
                return true;
            }
        }
    }
    public void WorkHasCeased()
    {
        lock (LockObject)
        {
            IsWorking = false;
            Ewh.Reset();
        }
    }
    public void SloppilyWaitForWorkToCease()
    {
        if (!IsWorking) return;
        lock (LockObject)
        {
            Ewh.WaitOne();
        }
    }
}