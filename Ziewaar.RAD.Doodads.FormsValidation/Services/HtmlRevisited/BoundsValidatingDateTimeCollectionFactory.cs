namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class BoundsValidatingDateTimeCollectionFactory(string[] lbounds, string[] ubounds) : IValidatingCollectionFactory
{
    private readonly DateTime
        LBound = lbounds.Select(DateTime.Parse).Concat([DateTime.MinValue]).Max(),
        UBound = ubounds.Select(DateTime.Parse).Concat([DateTime.MaxValue]).Min();
    public IValidatingCollection Create() => new DateTimeBoundsValidatingCollection(LBound, UBound);
}