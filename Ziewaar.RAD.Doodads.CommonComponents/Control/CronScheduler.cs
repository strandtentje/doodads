using CronNET;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Schedule work using cron expression")]
[Description("""
             Stick a cron expression in register or primary constant, and OnThen will be triggered on every match.
             """)]
public class CronScheduler : BasicService
{
    private static readonly object CronLock = new object();
    private static CronDaemon Crongleton
    {
        get
        {
            lock (CronLock)
            {
                if (field == null)
                {
                    field = new CronDaemon();
                    field.Start();
                }
                return field;
            }
        }
    }
    public override event CallForInteraction? OnThen;
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForNullOrEmpty(PrimaryOrRegister(constants, interaction), "cron expression required", out string cronExpression);
        Crongleton.AddJob(cronExpression, () =>
        {
            OnThen?.Invoke(this, interaction);
        });
    }
}