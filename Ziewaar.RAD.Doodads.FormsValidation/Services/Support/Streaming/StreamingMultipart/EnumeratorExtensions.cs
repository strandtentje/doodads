using System.Text;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;
public static class EnumeratorExtensions
{
    public static IEnumerable<byte> Collapse(this IEnumerator<byte> enumerator)
    {
        while (enumerator.MoveNext())
            yield return enumerator.Current;
    }
    public static string ToAscii(this IEnumerator<byte> enumerator) =>
        new(Encoding.ASCII.GetString(enumerator.Collapse().ToArray()));
}