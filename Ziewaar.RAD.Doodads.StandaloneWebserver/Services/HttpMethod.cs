using System.Runtime.InteropServices;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
[Category("Http & Routing")]
[Title("Match HTTP Method")]
[Description("""
             Does a hard check against a certain HTTP method.
             """)]
public class HttpMethod : IService
{
    [PrimarySetting("Method name to filter for ie. POST, GET, PUT, HEAD, etc.")]
    private readonly UpdatingPrimaryValue MethodNameConstant = new();
    private System.Net.Http.HttpMethod? CurrentMethod;
    [EventOccasion("When the request method is matching")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the request method is not matching")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when the method was strange, or there was no request.")]
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