using System.Globalization;

namespace Define.Doodads.Expo.Timeline
{
    public class BasicException(string msg) : Exception(msg)
    {
        public static void ForPopulatedArray(IEnumerable e, string message, out string[] items)
        {
            items = [.. e.OfType<string>()];
            if (items.Length == 0)
                throw new BasicException(message);
        }

        public static void ForNullOrEmpty(object? value, string message, out string str)
        {
            var stringed = value?.ToString();
            if (string.IsNullOrWhiteSpace(stringed))
                throw new BasicException(message);
            str = stringed!;
        }

        public static void ForDequeue(Queue<string> q, string message, out string value)
        {
            if (!q.Any())
                throw new BasicException(message);
            value = q.Dequeue();
            ForNullOrEmpty(value, message, out value);
        }

        public static void ForConstraint(string text, int min, int max, string message, out int value)
        {
            ForNullOrEmpty(text, message, out var _);
            if (!int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                throw new BasicException(message);
            if (value < min || value > max)
                throw new BasicException(message);
        }

        public static void ForConstraint(int value, int min, int max, string message)
        {
            if (value < min || value > max)
                throw new BasicException(message);
        }

        public static void ForDictContains<TValue>(IReadOnlyDictionary<string, TValue> dict, string name,
            string message, out TValue value)
        {
            if (!dict.TryGetValue(name, out value))
                throw new BasicException(message);
        }

        public static void ForDictNotContains<TValue>(IReadOnlyDictionary<string, TValue> dict, string name,
            string message)
        {
            if (dict.ContainsKey(name))
                throw new BasicException(message);
        }
    }
}