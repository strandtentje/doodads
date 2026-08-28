namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.DependencyDownloader
{
    [Serializable]
    public class BinaryDependencyRetrievalException(Exception inner, string? message = null) : Exception($"Json Download Exception for {message}", inner);
}
