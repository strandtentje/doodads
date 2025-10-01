using Ziewaar.RAD.Doodads.EnumerableStreaming;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
[Category("Http & Routing")]
[Title("Parse HTTP headers from text")]
[Description("Provided raw text in the register, extract http headers.")]
public class RawHttpHeaders : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register is not string headersString)
        {
            OnException?.Invoke(this, interaction.AppendRegister("string with headers required"));
            return;
        }

        var afterMethodIndex = headersString.IndexOf(" ", 0, Math.Min(10, headersString.Length),
            StringComparison.OrdinalIgnoreCase);
        if (afterMethodIndex < 0)
        {
            OnElse?.Invoke(this, interaction);
            return;
        }
        var supposedMethodWord = headersString.Substring(0, afterMethodIndex);
        if (System.Net.Http.HttpMethod.Parse(supposedMethodWord) is not { } foundMethod)
        {
            OnElse?.Invoke(this, interaction);
            return;
        }

        var endOfRouteIndex = headersString.IndexOf("\r\n", afterMethodIndex, StringComparison.OrdinalIgnoreCase);
        if (endOfRouteIndex < 0)
        {
            OnElse?.Invoke(this, interaction);
            return;
        }

        var endOfHeadersIndex = headersString.IndexOf("\r\n\r\n", endOfRouteIndex, StringComparison.OrdinalIgnoreCase);
        var supposedRoute = headersString.Substring(afterMethodIndex, endOfRouteIndex - afterMethodIndex);

        OnThen?.Invoke(this,
            interaction.AppendMemory(("rawmethod", foundMethod), ("rawurl", supposedRoute)).AppendMemory(
                new LazilyPrefixedCaseInsensitiveDictionary("rawheader_",
                    new HeaderReadingEnumerable(headersString, endOfRouteIndex, endOfHeadersIndex))));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}