using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Bounding;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Composite;

public class BoundsValidatingCollectionFactory(string fieldType, string[] lbounds, string[] ubounds)
    : IValidatingCollectionFactory
{
    private readonly IValidatingCollectionFactory? TrueFactory = fieldType switch
    {
        "date" => new BoundsValidatingDateOnlyCollectionFactory(lbounds, ubounds),
        "datetime-local" => new BoundsValidatingDateTimeCollectionFactory(lbounds, ubounds),
        "month" => new BoundsValidatingMonthCollectionFactory(lbounds, ubounds),
        "number" or "range" => new BoundsValidatingNumberCollectionFactory(lbounds, ubounds),
        "time" => new BoundsValidatingTimeCollectionFactory(lbounds, ubounds),
        "week" => new BoundsValidatingWeekCollectionFactory(lbounds, ubounds),
        _ => lbounds.Length != 0 || ubounds.Length != 0
            ? throw new ArgumentException("Cannot set bounds to unboundable field")
            : null,
    };
    public bool CanConstrain => TrueFactory?.CanConstrain == true;
    public IValidatingCollection? Create() => TrueFactory?.Create();
}