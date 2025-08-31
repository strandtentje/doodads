namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Bounding;
public class BoundsValidatingTimeCollectionFactory(string[] lbounds, string[] ubounds) : IValidatingCollectionFactory
{// Drop-in: replace fields in BoundsValidatingTimeCollectionFactory
    private static readonly string[] TimeFormats = new[]
    {
        "HH':'mm",
        "HH':'mm':'ss",
        "HH':'mm':'ss'.'FFFFFFF",
    };
    private readonly TimeOnly
        LBound = lbounds.Select(s => TimeOnly.ParseExact(s, TimeFormats, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None))
            .Concat(new[] { TimeOnly.MinValue }).Max(),
        UBound = ubounds.Select(s => TimeOnly.ParseExact(s, TimeFormats, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None))
            .Concat(new[] { TimeOnly.MaxValue }).Min();
    public IValidatingCollection Create() => new BoundsValidatingTimeCollection(LBound, UBound);
}