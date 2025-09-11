namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Bounding;
public class BoundsValidatingNumberCollectionFactory(string[] lbounds, string[] ubounds) : IValidatingCollectionFactory
{
    public bool CanConstrain => lbounds.Any() || ubounds.Any();
    private readonly decimal
        LBound = lbounds.Select(x => decimal.Parse(x, CultureInfo.InvariantCulture)).Concat([decimal.MinValue]).Max(),
        UBound = ubounds.Select(x => decimal.Parse(x, CultureInfo.InvariantCulture)).Concat([decimal.MaxValue]).Min();
    public IValidatingCollection Create() => new BoundsValidatingNumberCollection(LBound, UBound);
}