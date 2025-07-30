namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Routing;
public class RouteEvaluationInteraction(
    IInteraction parent,
    IEnumerable<string> components,
    string currentPath,
    HttpHeadInteraction head) : IInteraction
{
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public IEnumerable<string> Components => components;
    public string CurrentPath => currentPath;
    public HttpHeadInteraction Head => head;
}