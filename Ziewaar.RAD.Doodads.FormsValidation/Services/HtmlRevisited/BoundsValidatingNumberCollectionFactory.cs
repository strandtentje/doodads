namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class BoundsValidatingNumberCollectionFactory(string[] lbounds, string[] ubounds) : IValidatingCollectionFactory
{
    private readonly decimal
        LBound = lbounds.Select(decimal.Parse).Concat([decimal.MinValue]).Max(),
        UBound = ubounds.Select(decimal.Parse).Concat([decimal.MaxValue]).Min();
    public IValidatingCollection Create() => new NumberBoundsValidatingCollection(LBound, UBound);
}