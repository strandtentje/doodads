namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Cookies;
public class ComponentCookie(byte[] randomBytes, Guid uniqueId) : IComparable, IComparable<ComponentCookie>
{
    private const string VALID_HEX_CHARS = "0123456789abcdef";
    private const int
        RANDOM_BYTE_POS = 0,
        RANDOM_BYTE_COUNT = 32,
        RANDOM_HEX_POS = RANDOM_BYTE_POS * 2,
        RANDOM_HEX_COUNT = RANDOM_BYTE_COUNT * 2;
    private const int
        UNIQUE_BYTE_POS = RANDOM_BYTE_COUNT,
        UNIQUE_BYTE_COUNT = 16,
        UNIQUE_HEX_POS = UNIQUE_BYTE_POS * 2,
        UNIQUE_HEX_COUNT = UNIQUE_BYTE_COUNT * 2;
    private const int
        TOTAL_BYTE_COUNT = RANDOM_BYTE_COUNT + UNIQUE_BYTE_COUNT,
        TOTAL_HEX_COUNT = TOTAL_BYTE_COUNT * 2;
    private static readonly RandomNumberGenerator Generator = RandomNumberGenerator.Create();
    public Guid Unique => uniqueId;
    public byte[] Random => randomBytes;
    public static ComponentCookie CreateNew()
    {
        byte[] bytes = new byte[RANDOM_BYTE_COUNT];
        Generator.GetBytes(bytes, 0, bytes.Length);
        return new ComponentCookie(bytes, Guid.NewGuid());
    }
    public static bool TryParse(
        string candidateComponentCookie,
        [NotNullWhen(true)] out ComponentCookie? resultCookie)
    {
        if (candidateComponentCookie.Length != TOTAL_HEX_COUNT ||
            !candidateComponentCookie.All(VALID_HEX_CHARS.Contains))
        {
            resultCookie = null;
            return false;
        }
        var randomBytes = Convert.FromHexString(candidateComponentCookie.AsSpan(RANDOM_HEX_POS, RANDOM_HEX_COUNT));
        if (randomBytes.Length != RANDOM_BYTE_COUNT)
        {
            resultCookie = null;
            return false;
        }
        var uniqueBytes = Convert.FromHexString(candidateComponentCookie.AsSpan(UNIQUE_HEX_POS, UNIQUE_HEX_COUNT));
        if (uniqueBytes.Length != UNIQUE_BYTE_COUNT)
        {
            resultCookie = null;
            return false;
        }
        try
        {
            resultCookie = new ComponentCookie(randomBytes, new Guid(uniqueBytes));
            return true;
        }
        catch (Exception)
        {
            resultCookie = null;
            return false;
        }
    }
    public override string ToString()
    {
        if (randomBytes.Length != RANDOM_BYTE_COUNT)
            throw new InvalidOperationException("Random padding was more than 32 bytes");
        var uniqueBytes = this.Unique.ToByteArray();
        if (uniqueBytes.Length != UNIQUE_BYTE_COUNT)
            throw new InvalidOperationException("GUID was more than 16 bytes.");
        var compoundBytes = new byte[RANDOM_BYTE_COUNT + UNIQUE_BYTE_COUNT];
        Array.Copy(
            sourceArray: randomBytes, sourceIndex: 0,
            destinationArray: compoundBytes,
            destinationIndex: RANDOM_BYTE_POS, length: RANDOM_BYTE_COUNT);
        Array.Copy(
            sourceArray: uniqueBytes, sourceIndex: 0,
            destinationArray: compoundBytes,
            destinationIndex: UNIQUE_BYTE_POS, length: UNIQUE_BYTE_COUNT);
        var compoundString = Convert.ToHexStringLower(compoundBytes);
        var filteredCharacters = compoundString.Where(VALID_HEX_CHARS.Contains).Take(TOTAL_HEX_COUNT).ToArray();
        if (filteredCharacters.Length != TOTAL_HEX_COUNT)
            throw new InvalidOperationException("Unexpected hex length");
        var filteredString = new string(filteredCharacters);
        return filteredString;
    }
    public int CompareTo(ComponentCookie? other)
    {
        if (other == null)
            return 1;
        var randomByteComparison = this.Random.Zip(other.Random, (l, r) => l.CompareTo(r));
        var firstDifference = randomByteComparison.FirstOrDefault();
        if (firstDifference != 0)
            return firstDifference;
        else
            return this.Unique.CompareTo(other.Unique);
    }
    public int CompareTo(object? obj)
    {
        if (obj is ComponentCookie cc)
            return ((IComparable<ComponentCookie>)this).CompareTo(cc);
        else
            throw new InvalidOperationException("ComponentCookie may not exist in mixed list.");
    }
}