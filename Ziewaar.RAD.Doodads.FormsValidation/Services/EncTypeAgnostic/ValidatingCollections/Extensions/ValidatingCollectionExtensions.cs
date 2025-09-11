namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Extensions;
public static class ValidatingCollectionExtensions
{
    public static void Add(this IValidatingCollection collection, object value) => collection.Add(value, out var _);
}