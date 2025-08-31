using System.Collections.Concurrent;
using HttpMultipartParser;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class StreamingMultipartFieldMap : IReadOnlyDictionary<string, IEnumerable>
{
    private readonly Dictionary<string, FieldStreamBuffer> _fieldBuffers = new();
    private readonly object _lock = new();

    public StreamingMultipartFieldMap(StreamingMultipartFormDataParser parser)
    {
        parser.ParameterHandler += ParameterHandler;

        parser.FileHandler += FileHandler;
    }

    private void FileHandler(
        string name, string fileName, string contentType, 
        string contentDisposition, 
        byte[] buffer, int bytes, int partNumber, 
        IDictionary<string, string> additionalProperties)
    {
        
        lock (_lock)
        {
            if (!_fieldBuffers.TryGetValue(name, out var buf))
                _fieldBuffers[name] = buf = new FieldStreamBuffer();
            buf.Add(new FileChunk(fileName, contentType, buffer[..bytes]));
        }
    }

    private void ParameterHandler(ParameterPart part)
    {
        lock (_lock)
        {
            if (!_fieldBuffers.TryGetValue(part.Name, out var buf))
                _fieldBuffers[part.Name] = buf = new FieldStreamBuffer();
            buf.Add(part.Data);
        }
    }

    public IEnumerable GetEnumeratorForKey(string key)
    {
        if (_fieldBuffers.TryGetValue(key, out var buf))
            return buf;
        return Enumerable.Empty<object>(); // or string/Stream/etc.
    }

    public IEnumerable<string> Keys => throw new NotSupportedException("Keys enumeration not supported");
    public IEnumerable<IEnumerable> Values => throw new NotSupportedException("Values enumeration not supported");
    public int Count => throw new NotSupportedException("Count not supported");
    public bool ContainsKey(string key) => throw new NotSupportedException();
    public bool TryGetValue(string key, out IEnumerable value) => throw new  NotSupportedException();
    public IEnumerable this[string key] => throw new NotSupportedException("Get by key not supported");
    public IEnumerable GetEnumerator() =>
        _fieldBuffers.Select(kvp => new KeyValuePair<string, IEnumerable>(kvp.Key, kvp.Value));
    IEnumerator<KeyValuePair<string, IEnumerable>> IEnumerable<KeyValuePair<string, IEnumerable>>.GetEnumerator() =>
        _fieldBuffers.Select(kvp => new KeyValuePair<string, IEnumerable>(kvp.Key, kvp.Value)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        ((IEnumerable<KeyValuePair<string, IEnumerable>>)this).GetEnumerator();

    // Internal class to buffer per-field values lazily
    private class FieldStreamBuffer : IEnumerable<object>
    {
        private readonly BlockingCollection<object> _buffer = new();

        public void Add(object val) => _buffer.Add(val);

        public IEnumerator<object> GetEnumerator()
        {
            foreach (var item in _buffer.GetConsumingEnumerable())
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // For file chunks or metadata; you could replace with full Stream logic
    public record FileChunk(string Filename, string ContentType, byte[] Data);
}