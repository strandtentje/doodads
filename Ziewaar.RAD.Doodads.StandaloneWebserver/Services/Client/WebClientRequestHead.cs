namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Client;
#pragma warning disable 67
public record WebClientRequestHead(
    System.Net.Http.HttpMethod Method,
    string URL,
    Func<HttpRequestMessage, HttpRequestMessage> ApplyModifications);