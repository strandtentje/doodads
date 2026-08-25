using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ziewaar.RAD.Doodads.Data.FlatFile.Support;

public static class JsonFileExtensions
{
    extension(JsonFile file)
    {
        public IReadOnlyDictionary<string, object> Set(string key, string member, object value) => file.Set(key, (member, value));
        public IReadOnlyDictionary<string, object> Set(string key, params (string member, object value)[] values) =>
            file.Set(key, (IEnumerable<(string, object)>)values);
        public IReadOnlyDictionary<string, object> Set(string key, IReadOnlyDictionary<string, object> values) =>
            file.Set(key, values.Select(x => (x.Key, x.Value)));
        public IReadOnlyDictionary<string, object> Overwrite(string key, IReadOnlyDictionary<string, object> values) =>
            file.Set(key, values.Select(x => (x.Key, x.Value)), true);
        public bool TryForKey(string key, out IReadOnlyDictionary<string, object>? dict)
        {
            dict = file.ForKey(key);
            return dict != null;
        }
    }
}