namespace Ziewaar.Web.Uploadserver.Pages;

public class DynamicPage
{
    private readonly IEnumerable<DynamicPageInstruction> Instructions;
    public DynamicPage(
        IEnumerable<DynamicPageInstruction> instructions)
    {
        this.Instructions = instructions;
    }
    internal static DynamicPage For(string file)
    {
        return new DynamicPage(
            new DynamicPageInstructionEnumerable(
                file));
    }
    public void Render(HttpListenerResponse response,
        IPlaceholderResolver resolver)
    {
        long contentLength = 0;
        response.ContentEncoding ??= Encoding.UTF8;
        foreach (var inst in Instructions)
        {
            if (inst.IsPlaceholder)
                contentLength += resolver.GetByteLengthFor(response.ContentEncoding, inst.GetComponent());
            else
                contentLength += inst.GetVerbatimBytes().Length;
        }

        if (resolver.IsVirgin)
        {
            response.ContentLength64 = contentLength;
            response.ContentType ??= "text/html";
        }

        foreach (var inst in Instructions)
        {
            if (inst.IsPlaceholder)
                resolver.WriteBytesFor(Encoding.UTF8, inst.GetComponent(), response.OutputStream);
            else
            {
                var b = inst.GetVerbatimBytes();
                response.OutputStream.Write(b, 0, b.Length);
            }
        }
    }
}