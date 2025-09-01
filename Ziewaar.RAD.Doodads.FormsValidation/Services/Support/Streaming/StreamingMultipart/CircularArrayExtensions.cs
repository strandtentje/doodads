namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;
public static class CircularArrayExtensions
{
    public static T GetCircular<T>(this T[] array, long index)
    {
        if (array == null || array.Length == 0)
            throw new ArgumentException("Array must not be null or empty.", nameof(array));

        long wrappedIndex = ((index % array.Length) + array.Length) % array.Length;
        return array[wrappedIndex];
    }

    public static T[] SetCircular<T>(this T[] array, long index, T value)
    {
        if (array == null || array.Length == 0)
            throw new ArgumentException("Array must not be null or empty.", nameof(array));

        long wrappedIndex = ((index % array.Length) + array.Length) % array.Length;
        array[wrappedIndex] = value;
        return array;
    }
}