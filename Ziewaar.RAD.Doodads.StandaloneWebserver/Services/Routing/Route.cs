namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Routing;
[Category("Http & Routing")]
[Title("Match (parent) route")]
[Description("""
             Will by default route GET only
             Will route single or multiple path segments.
             Will route relative to the previous Route expression,
             or absolutely regardless of what routing happened before.
             Can take route parameters. 
             Methods may be specified, in place of a method ANY can be specified to allow any method
             
             ```Route("/about")```
             ```Route("/about"):Route("GET mission")```
             ```Route("/"):Route("ANY about/mission")```
             will match `/about` and `/about/mission`
             
             ```Route("/product/{#id#}/{$color$}")```
             Will match `/product/123/blue` and stick `123` into memory at `id`, and `blue` into memory at `color`
             Will not match `/product/blue/123` because {##} means parsable decimal with . and {$$} means text.
             As such, `/product/123/123` will match.
             
             ```Route("POST /purchase")```
             Will only match POST requests on `/purchase` and its subdirs
             
             ```ExactRoute("POST /purchase")```
             Will only match POST requests on `/purchase` and no subdirs.
             """)]
public class Route : IService
{
    [PrimarySetting("Routing template as specified in the description above")]
    private readonly UpdatingPrimaryValue RouteTemplateConst = new();
    [EventOccasion("When the route matched")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the route did not match")]
    public event CallForInteraction? OnElse;
    [EventOccasion("When the route template was wrong, or a relative Route was defined before an absolute Route was defined.")]
    public event CallForInteraction? OnException;
    private readonly List<IRouteComponent> RouteTemplateComponents = new();
    private bool IsAbsoluteRoute;
    private string CurrentMethod = "GET";
    protected virtual bool IsExact => false;
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

        if (RouteTemplateComponents.Count > componentArray.Length || IsExact && RouteTemplateComponents.Count != componentArray.Length)
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