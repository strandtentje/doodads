namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class BoundsValidatingCollectionFactory(string fieldType, string[] lbounds, string[] ubounds)
    : IValidatingCollectionFactory
{
    private readonly IValidatingCollectionFactory TrueFactory = fieldType switch
    {
        "date" => new BoundsValidatingDateOnlyCollectionFactory(lbounds, ubounds),
        "datetime-local" => new BoundsValidatingDateTimeCollectionFactory(lbounds, ubounds),
        "month" => new BoundsValidatingMonthCollectionFactory(lbounds, ubounds),
        "number" or "range" => new BoundsValidatingNumberCollectionFactory(lbounds, ubounds),
        "time" => new BoundsValidatingTimeCollectionFactory(lbounds, ubounds),
        "week" => new BoundsValidatingWeekCollectionFactory(lbounds, ubounds),
        _ => new NonValidatingCollectionFactory(),
    };
    public IValidatingCollection Create() => TrueFactory.Create();
}