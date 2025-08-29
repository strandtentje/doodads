namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class BoundsValidatingDateOnlyCollectionFactory(string[] lbounds, string[] ubounds) : IValidatingCollectionFactory
{
    private readonly DateOnly
        LBound = lbounds.Select(DateOnly.Parse).Concat([DateOnly.MinValue]).Max(),
        UBound = ubounds.Select(DateOnly.Parse).Concat([DateOnly.MaxValue]).Min();
    public IValidatingCollection Create() => new DateFieldBoundsValidatingCollection(LBound, UBound);
}