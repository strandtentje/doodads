using System.Net;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces
{
    public interface IMayRedirectInteraction : IInteraction
    {
        void SetRedirection(HttpStatusCode statusCode, string url);
        bool IsRedirecting { get; }
    }
}