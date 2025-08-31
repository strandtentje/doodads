namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class BoundsValidatingDateTimeCollectionFactory(string[] lbounds, string[] ubounds) : IValidatingCollectionFactory
{
    private static readonly string[] DateTimeLocalFormats = new[]
    {
        "yyyy'-'MM'-'dd'T'HH':'mm",
        "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
        "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFF",
    };

    private readonly DateTime
        LBound = lbounds.Select(s => DateTime.ParseExact(s, DateTimeLocalFormats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None))
            .Concat(new[] { DateTime.MinValue }).Max(),
        UBound = ubounds.Select(s => DateTime.ParseExact(s, DateTimeLocalFormats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None))
            .Concat(new[] { DateTime.MaxValue }).Min();
    public IValidatingCollection Create() => new BoundsValidatingDateTimeCollection(LBound, UBound);
}