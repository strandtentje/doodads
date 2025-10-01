namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services
{
#pragma warning disable 67
    public class HeaderReadingEnumerable(string headerText, int startOfHeaders, int endOfHeaders)
        : IEnumerable<KeyValuePair<string, object>>
    {
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            int position = startOfHeaders;
            while (true)
            {
                position += 2;
                var endOfKey = headerText.IndexOf(':', position, endOfHeaders - position);
                if (endOfKey <= position)
                    yield break;

                var endOfValue =
                    headerText.IndexOf("\r\n", position, endOfHeaders - position, StringComparison.Ordinal);
                if (endOfValue <= endOfKey)
                    yield break;

                var key = headerText.Substring(position, endOfKey - position);
                var value = headerText.Substring(endOfKey + 1, endOfValue - endOfKey - 1).Trim();
                yield return new KeyValuePair<string, object>(key, value);
                position = endOfValue;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}