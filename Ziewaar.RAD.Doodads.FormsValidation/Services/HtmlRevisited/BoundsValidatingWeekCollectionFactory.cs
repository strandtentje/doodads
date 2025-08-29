namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class BoundsValidatingWeekCollectionFactory(string[] lbounds, string[] ubounds) : IValidatingCollectionFactory
{
    private readonly DateOnly
        LBound = lbounds.Select(WeekOnly.Parse).Select(x => x.ToDateOnly()).Concat([DateOnly.MinValue]).Max(),
        UBound = ubounds.Select(WeekOnly.Parse).Select(x => x.ToDateOnly()).Concat([DateOnly.MaxValue]).Min();
    public IValidatingCollection Create() => new BoundsValidatingWeekCollection(LBound, UBound);
}