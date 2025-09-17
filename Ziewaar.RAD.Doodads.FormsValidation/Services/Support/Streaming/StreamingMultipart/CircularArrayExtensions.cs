namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;
public static class CircularArrayExtensions
{
    public static T GetCircular<T>(this T[] array, long index)
    {
        long wrappedIndex = ((index % array.Length) + array.Length) % array.Length;
        return array[wrappedIndex];
    }

    public static T[] SetCircular<T>(this T[] array, long index, T value)
    {
        long wrappedIndex = ((index % array.Length) + array.Length) % array.Length;
        array[wrappedIndex] = value;
        return array;
    }
}