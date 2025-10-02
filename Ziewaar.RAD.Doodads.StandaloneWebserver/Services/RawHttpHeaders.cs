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

        var preBreakString = headersString.Split(["\r\n\r\n"], count: 2, options: StringSplitOptions.None)
            .ElementAtOrDefault(0) ?? "";
        var headerLines = preBreakString.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries);
        var headerArrays = headerLines.Select(line => line.Split(':', count: 2)).Where(x => x.Length == 2);
        var headerPairs = headerArrays.Select(array => (key: array[0], value: array[1]));
        var distinctPairs = headerPairs.DistinctBy(x => x.key);
        var headerDict = distinctPairs.ToDictionary(x => $"rawheader_{x.key}", x => x.value as object,
            StringComparer.OrdinalIgnoreCase);

        OnThen?.Invoke(this, interaction.AppendMemory(dict: headerDict));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}