#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection.Crud;
#pragma warning disable 67
public class DetectServiceOrSeries : IService
{
    public event CallForInteraction? OnAlso;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnService;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        switch (interaction.Register)
        {
            case UnconditionalSerializableServiceSeries<ServiceBuilder> ampersandExpression:
                OnAlso?.Invoke(this, new CommonInteraction(interaction, ampersandExpression));
                break;
            case AlternativeSerializableServiceSeries<ServiceBuilder> pipeExpression:
                OnElse?.Invoke(this, new CommonInteraction(interaction, pipeExpression));
                break;
            case ConditionalSerializableServiceSeries<ServiceBuilder> colonExpression:
                OnThen?.Invoke(this, new CommonInteraction(interaction, colonExpression));
                break;
            case ServiceDescription<ServiceBuilder> serviceExpression:
                OnService?.Invoke(this, new CommonInteraction(interaction, serviceExpression));
                break;
            default:
                OnException?.Invoke(this,
                    new CommonInteraction(interaction, "Expected service expression from ie DefinitionSeries"));
                break;
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
