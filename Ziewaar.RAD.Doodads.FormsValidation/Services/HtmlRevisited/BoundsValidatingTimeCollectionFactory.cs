namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class BoundsValidatingTimeCollectionFactory(string[] lbounds, string[] ubounds) : IValidatingCollectionFactory
{
    private readonly TimeOnly
        LBound = lbounds.Select(TimeOnly.Parse).Concat([TimeOnly.MinValue]).Max(),
        UBound = ubounds.Select(TimeOnly.Parse).Concat([TimeOnly.MaxValue]).Min();
    public IValidatingCollection Create() => new TimeFieldBoundsValidatingCollection(LBound, UBound);
}