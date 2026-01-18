namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Routing;
[Category("Http & Routing")]
[Title("Match route exactly")]
[Description("Behaves like Route, but will only match if the route matches exactly, without subdirectories.")]
[ShortNames("er")]
public class ExactRoute : Route
{
    protected override bool IsExact => true;
}