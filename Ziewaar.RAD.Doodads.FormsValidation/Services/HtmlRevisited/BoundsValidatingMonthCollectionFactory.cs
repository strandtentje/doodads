namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class BoundsValidatingMonthCollectionFactory(string[] lbounds, string[] ubounds) : IValidatingCollectionFactory
{
    private DateOnly 
        LBound = lbounds.Select(DateOnly.Parse).Concat([DateOnly.MinValue]).Max(),
        UBound = ubounds.Select(DateOnly.Parse).Concat([DateOnly.MaxValue]).Min();
    public IValidatingCollection Create() => new MonthBoundsValidatingCollection(LBound, UBound);
}