using System.Runtime.InteropServices;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public class HttpMethod : IService
{
    private readonly UpdatingPrimaryValue MethodNameConstant = new();
    private System.Net.Http.HttpMethod? CurrentMethod;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, MethodNameConstant).IsRereadRequired(
                () => "POST", out string? candidateMethodName) &&
            !string.IsNullOrWhiteSpace(candidateMethodName))
        {
            try
            {
                this.CurrentMethod = System.Net.Http.HttpMethod.Parse(candidateMethodName.ToUpper());
            }
            catch (Exception)
            {
                this.CurrentMethod = System.Net.Http.HttpMethod.Post;
            }
        }
        if (this.CurrentMethod == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Unknown method"));
            return;
        }
        if (!interaction.TryGetClosest<HttpHeadInteraction>(out var headInteraction) || headInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "No http head to check method"));
            return;
        }
        if (string.Equals(headInteraction.Method, this.CurrentMethod.ToString(),
                StringComparison.CurrentCultureIgnoreCase))
            OnThen?.Invoke(this, interaction);
        else
            OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}