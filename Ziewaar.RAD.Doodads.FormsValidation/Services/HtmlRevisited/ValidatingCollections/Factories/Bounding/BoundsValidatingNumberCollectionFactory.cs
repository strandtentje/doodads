namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class BoundsValidatingNumberCollectionFactory(string[] lbounds, string[] ubounds) : IValidatingCollectionFactory
{
    private readonly decimal
        LBound = lbounds.Select(x => decimal.Parse(x, CultureInfo.InvariantCulture)).Concat([decimal.MinValue]).Max(),
        UBound = ubounds.Select(x => decimal.Parse(x, CultureInfo.InvariantCulture)).Concat([decimal.MaxValue]).Min();
    public IValidatingCollection Create() => new BoundsValidatingNumberCollection(LBound, UBound);
}