#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

public class CacheValue
{
    private object AccessLock = new();
    private string? MimeType;
    private IEnumerable<object>? StoredValidationValues { get; set; }
    private byte[]? Data { get; set; }

    public bool TryValidateValues(IEnumerable<object> currentValidationValues)
    {
        lock (AccessLock)
        {
            if (this.StoredValidationValues == null)
                throw new InvalidOperationException("Cannot validate cache entry with unset validation keys");
            var privateEnumerable = this.StoredValidationValues as IReadOnlyList<object> ?? this.StoredValidationValues.ToList();
            if (privateEnumerable.Count == 0)
                throw new InvalidOperationException("Cannot validate cache entry with zero validation keys");
            var providedEnumerable = currentValidationValues as IReadOnlyList<object> ?? currentValidationValues.ToList();
            if (providedEnumerable.Count == 0)
                throw new ArgumentException(
                    "Cannot validate cache entry; no validation keys provided to validate against.");
            return this.StoredValidationValues.SequenceEqual(providedEnumerable);
        }
    }

    public void CopyTo(Stream stream, out string mime)
    {
        if (!stream.CanWrite) throw new InvalidOperationException("Stream must be writable");
        if (Data is null) throw new InvalidOperationException("Data must be set.");
        if (this.MimeType is null) throw new InvalidOperationException("Mimetype must be set.");
        stream.Write(this.Data, 0, this.Data.Length);
        mime = this.MimeType;
    }

    public void SetDataAndValues(string mime, IEnumerable<object> validateKeys, byte[] getData)
    {
        lock (AccessLock)
        {
            if (this.MimeType != null || this.StoredValidationValues != null || this.Data != null)
                throw new InvalidOperationException("May only set data once on cache entry.");
            this.MimeType = mime;
            this.StoredValidationValues = validateKeys;
            this.Data = getData;
        }
    }
}