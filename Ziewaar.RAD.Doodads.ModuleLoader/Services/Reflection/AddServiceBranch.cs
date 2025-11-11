namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
#nullable  enable
#pragma warning disable 67
public class AddServiceBranch : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryFindVariable(ReflectionKeys.Service,
                out ServiceDescription<ServiceBuilder>? service) ||
            service == null)
        {
            OnException?.Invoke(this,
                interaction.AppendRegister(
                    "can only do this on a service"));
            return;
        }

        if (service.LastCursorText == null ||
            service.CurrentNameInScope == null)
        {
            OnException?.Invoke(this,
                interaction.AppendRegister(
                    "can only do this on existing file"));
            return;
        }

        var branchName = interaction.Register.ToString();
        if (service.Children.Branches != null)
        {
            if (service.Children.Branches.Any(x => x.key == branchName))
            {
                OnException?.Invoke(this, interaction.AppendRegister("branch already exists"));
                return;
            }
            var newCursor = CursorText.Create(
                service.LastCursorText.WorkingDirectory,
                service.LastCursorText.BareFile, "VoidService()");
            var series =
                new UnconditionalSerializableServiceSeries<ServiceBuilder>();
            series.UpdateFrom(branchName, ref newCursor);

            service.Children.Branches.Add((branchName, series));
        }
        else
        {
            var newCursor = CursorText.Create(
                service.LastCursorText.WorkingDirectory,
                service.LastCursorText.BareFile,
                $"{branchName}->VoidService()");
            service.Children.UpdateFrom(ref newCursor);
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}