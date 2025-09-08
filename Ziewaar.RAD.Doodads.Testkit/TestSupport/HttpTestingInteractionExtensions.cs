using System.Text;

namespace Ziewaar.RAD.Doodads.Testkit;
public static class HttpTestingInteractionExtensions
{
    public static string ResponseAsString(this HttpTestingHarness hrn)
    {
        using StreamReader sr = new(hrn.Stream, Encoding.UTF8);
        return sr.ReadToEnd();
    }
}