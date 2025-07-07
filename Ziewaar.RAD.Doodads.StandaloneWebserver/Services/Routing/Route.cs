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
public class Route : IService
{
    private readonly UpdatingPrimaryValue RouteTemplateConst = new();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    private readonly List<IRouteComponent> RouteTemplateComponents = new();
    private bool IsAbsoluteRoute;
    private string CurrentMethod = "GET";
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RouteTemplateConst).IsRereadRequired(out string? routeWithMethodTemplate) &&
            routeWithMethodTemplate != null)
        {
            var methodAndRoute = routeWithMethodTemplate.Split(' ', 2, (StringSplitOptions)3);
            CurrentMethod = "GET";
            string justTheRoute = "/";
            if (methodAndRoute.Length == 1)
            {
                CurrentMethod = "GET";
                justTheRoute = methodAndRoute.ElementAtOrDefault(0) ?? "";
            }
            else if (methodAndRoute.Length > 1)
            {
                var configuredMethodText = (methodAndRoute.ElementAtOrDefault(0) ?? "GET").ToUpper().Trim();
                if (configuredMethodText == "ANY")
                {
                    CurrentMethod = configuredMethodText;
                }
                else
                {
                    try
                    {
                        CurrentMethod = System.Net.Http.HttpMethod.Parse(configuredMethodText).ToString()
                            .ToUpper();
                    }
                    catch (Exception)
                    {
                        CurrentMethod = "GET";
                    }
                }

                justTheRoute = string.Join('/', methodAndRoute.Skip(1));
            }

            this.IsAbsoluteRoute = justTheRoute.StartsWith('/');
            var routeTemplateParts = justTheRoute.Split('/', options: (StringSplitOptions)3);
            RouteTemplateComponents.Clear();
            foreach (var part in routeTemplateParts)
            {
                if (NumericComponent.TryParse(part, out var foundNumber))
                    RouteTemplateComponents.Add(foundNumber);
                else if (TextualComponent.TryParse(part, out var foundText))
                    RouteTemplateComponents.Add(foundText);
                else if (FixedComponent.TryParse(part, out var foundFixed))
                    RouteTemplateComponents.Add(foundFixed);
            }
        }

        RouteEvaluationInteraction? evaluation = interaction as RouteEvaluationInteraction;

        if (evaluation == null) // Route("/product/{productid}/details")
        {
            switch (IsAbsoluteRoute)
            {
                case true when interaction.TryGetClosest<HttpHeadInteraction>(out var headInteraction) &&
                               headInteraction != null:
                    evaluation = new(interaction,
                        headInteraction.RouteString.Split('/', options: (StringSplitOptions)3),
                        "/",
                        headInteraction);
                    break;
                case false when
                    interaction.TryGetClosest<RelativeRouteInteraction>(out var relativeRouteInteraction) &&
                    relativeRouteInteraction != null:
                    evaluation = new(interaction,
                        relativeRouteInteraction.Remaining,
                        relativeRouteInteraction.CurrentLocation,
                        relativeRouteInteraction.HttpHead);
                    break;
                default:
                    OnException?.Invoke(this, new CommonInteraction(interaction,
                        """
                        Routing must be absolute or relative, that means
                        - Either specify a route template that starts with a slash (/) and has no preceeding routing
                        - Or specify a route template that doesn't start with a slash and has preceeding routing.
                        """));
                    return;
            }
        }

        if (evaluation.Head.Method != CurrentMethod && CurrentMethod != "ANY")
        {
            OnElse?.Invoke(this, evaluation);
            return;
        }

        var componentArray = evaluation.Components.ToArray();

        SortedList<string, object> urlVariables = new();
        var currentPath = new StringBuilder(evaluation.CurrentPath);
        int currentPositionInUrlComponents = 0;

        if (RouteTemplateComponents.Count > componentArray.Length)
        {
            OnElse?.Invoke(this, evaluation);
            return;
        }

        foreach (var component in RouteTemplateComponents)
        {
            if (component.TryParseSegment(componentArray[currentPositionInUrlComponents], urlVariables, out string verbatim))
            {
                currentPath.Append(verbatim).Append('/');
            }
            else
            {
                OnElse?.Invoke(this, evaluation);
                return;
            }
            currentPositionInUrlComponents++;
        }

        string[] remainingUrlComponents = [.. componentArray.Skip(currentPositionInUrlComponents)];
        OnThen?.Invoke(this, new RelativeRouteInteraction(
            interaction,
            evaluation.Head,
            urlVariables,
            currentPath.ToString(),
            remainingUrlComponents));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}