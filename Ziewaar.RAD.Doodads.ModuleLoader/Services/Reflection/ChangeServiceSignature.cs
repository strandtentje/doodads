namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
#nullable  enable
#pragma warning disable 67
public class ChangeServiceSignature : IService
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

        var tsi = new TextSinkingInteraction(interaction);
        OnThen?.Invoke(this, tsi);
        if (service.LastCursorText == null ||
            service.CurrentNameInScope == null)
        {
            OnException?.Invoke(this,
                interaction.AppendRegister(
                    "can only do this on existing file"));
            return;
        }

        var newCursor = CursorText.Create(
            service.LastCursorText.WorkingDirectory,
            service.LastCursorText.BareFile, tsi.ReadAllText());
        service.UpdateFrom(service.CurrentNameInScope, ref newCursor);
    }

    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}