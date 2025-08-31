using System.Text;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations;
public class LengthValidatorCollection(uint minLength, uint maxLength) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied) return;
        string asString;
        var sbl = new StringBuilder();
        if (value is IEnumerator<char> enumerator)
        {
            while (enumerator.MoveNext())
            {
                sbl.Append(enumerator.Current);
                if (sbl.Length > maxLength)
                {
                    IsSatisfied = false;
                    return;
                }
            }
            if (sbl.Length < minLength)
            {
                IsSatisfied = false;
                return;
            }
            asString = sbl.ToString();
        }
        else
        {
            asString = value.ToString() ?? "";
            if (asString.Length < minLength)
                IsSatisfied = false;
            if (asString.Length > maxLength)
                IsSatisfied = false;
        }
        if (IsSatisfied)
            BackingValues.Add(transformed = asString);
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}