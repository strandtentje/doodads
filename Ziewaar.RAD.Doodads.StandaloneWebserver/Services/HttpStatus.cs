namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
[Category("Http & Routing")]
[Title("Set HTTP Status code")]
[Description("""
             Force the HTTP status code.
             """)]
public class HttpStatus : IService
{
    [PrimarySetting("Status code number")]
    private readonly UpdatingPrimaryValue HttpStatusCodeConstant = new();
    private int CurrentStatusCode;
    [EventOccasion("When the status code was set")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when we're trying to set status on something else than a webserver")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, HttpStatusCodeConstant).IsRereadRequired(out object? statusCode))
        {
            if (Enum.TryParse<HttpStatusCode>(statusCode?.ToString() ?? "", out var parsedStatusCode))
            {
                CurrentStatusCode = (int)parsedStatusCode;
            }
            else
            {
                this.CurrentStatusCode = Convert.ToInt32(statusCode);
            }
        }

        if (!interaction.TryGetClosest<HttpResponseInteraction>(out var httpResponseInteraction) ||
            httpResponseInteraction == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "Can only set status code on HTTP request; which is not available."));
            return;
        }
        OnThen?.Invoke(this, interaction);
        httpResponseInteraction.StatusCode = CurrentStatusCode;
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}