using System.Text;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;
public static class StringExtensions
{
    public static string MakeCrlf(this string original)
    {
        StringReader reader = new StringReader(original);
        StringBuilder sbl = new(original.Length * 2);
        while (reader.ReadLine() is string newLine)
        {
            sbl.Append(newLine);
            sbl.Append("\r\n");
        }
        return sbl.ToString();
    }
}