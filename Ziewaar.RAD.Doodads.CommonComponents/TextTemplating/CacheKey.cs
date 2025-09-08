#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

public class CacheKey(IEnumerable<object> identityKeys) : IComparable<CacheKey>, IComparable
{
    public IEnumerable<object> Members => identityKeys; 
    public int CompareTo(CacheKey other)
    {
        var internalKeys = identityKeys as IReadOnlyList<object> ?? identityKeys.ToList();
        var externalKeys = other.Members as IReadOnlyList<object> ??  other.Members.ToList();
        var primaryComparison = internalKeys.Count.CompareTo(externalKeys.Count);
        if (primaryComparison != 0) return primaryComparison;
        else return CompareEnumerables(0, internalKeys, externalKeys);
    }
    private static int CompareEnumerables(int offset, IReadOnlyList<object> x, IReadOnlyList<object> y)
    {
        if (x.Count != y.Count) throw new ArgumentException("May only compare to arrays of equal length");
        if (offset >= x.Count) 
            return 0;

        int firstComparison = x[offset] is not IComparable xComparable || y[offset] is not IComparable yComparable
            ? String.Compare(x[offset].ToString(), y[offset].ToString(), StringComparison.Ordinal)
            : xComparable.CompareTo(yComparable);

        return firstComparison == 0 ? CompareEnumerables(offset + 1, x, y) : firstComparison;
    }
    public int CompareTo(object obj)
    {
        if (obj is CacheKey other) return this.CompareTo(other);
        else throw new ArgumentException("May only compare two cache keys.");
    }
}