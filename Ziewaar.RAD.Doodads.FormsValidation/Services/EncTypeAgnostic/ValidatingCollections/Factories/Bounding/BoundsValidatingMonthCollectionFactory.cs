namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Bounding;
public class BoundsValidatingMonthCollectionFactory(string[] lbounds, string[] ubounds) : IValidatingCollectionFactory
{
    public bool CanConstrain => lbounds.Any() || ubounds.Any();
    private DateOnly 
        LBound = lbounds.Select(DateOnly.Parse).Concat([DateOnly.MinValue]).Max(),
        UBound = ubounds.Select(DateOnly.Parse).Concat([DateOnly.MaxValue]).Min();
    public IValidatingCollection Create() => new BoundsValidatingMonthCollection(LBound, UBound);
}