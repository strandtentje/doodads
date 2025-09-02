namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Bounding;
public class BoundsValidatingDateOnlyCollectionFactory(string[] lbounds, string[] ubounds) : IValidatingCollectionFactory
{
    private static readonly string[] DateFormats = new[]
    {
        "yyyy'-'MM'-'dd"
    };

    private readonly DateOnly
        LBound = lbounds.Select(s => DateOnly.ParseExact(s, DateFormats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None))
            .Concat(new[] { DateOnly.MinValue }).Max(),
        UBound = ubounds.Select(s => DateOnly.ParseExact(s, DateFormats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None))
            .Concat(new[] { DateOnly.MaxValue }).Min();

    public bool CanConstrain => lbounds.Any() || ubounds.Any();
    public IValidatingCollection Create() => new BoundsValidatingDateCollection(LBound, UBound);
}