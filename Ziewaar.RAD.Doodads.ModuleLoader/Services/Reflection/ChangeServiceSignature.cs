namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
#nullable  enable
#pragma warning disable 67
[Category("Reflection & Documentation")]
[Title("Change Service(Signature=1)")]
[Description("Provided a service signature, will change it for the" +
             "currently scoped service.")]
public class ChangeServiceSignature : IService
{
    [EventOccasion("Sink new service signature here.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When there's no service in scope, or we're not working in an existing file.")]
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