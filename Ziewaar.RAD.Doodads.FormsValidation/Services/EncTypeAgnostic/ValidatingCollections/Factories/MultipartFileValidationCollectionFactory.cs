using System.Collections.Immutable;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;

public class MultipartFileValidationCollectionFactory(string expectedName, string acceptAttribute)
    : IValidatingCollectionFactory
{
    public bool CanConstrain => true;
    public IValidatingCollection Create()
    {
        var attributeComponents = acceptAttribute.Split(',', (StringSplitOptions)3);
        var extensions = attributeComponents.Where(x => x.StartsWith('.'))
            .ToImmutableSortedSet(StringComparer.OrdinalIgnoreCase);
        var mimetypes = attributeComponents.Where(x => x.IndexOf('/') > -1)
            .ToImmutableSortedSet(StringComparer.OrdinalIgnoreCase);
        return new MultipartFileValidatingCollection(expectedName, extensions, mimetypes);
    }
}