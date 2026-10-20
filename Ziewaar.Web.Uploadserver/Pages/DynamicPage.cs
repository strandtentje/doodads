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
        foreach (var inst in Instructions)
        {
            if (inst.IsPlaceholder)
                contentLength += resolver.GetByteLengthFor(inst.GetComponent());
            else
                contentLength += inst.GetVerbatimBytes().Length;
        }
        response.ContentLength64 = contentLength;
        response.ContentType = "text/html";
        foreach (var inst in Instructions)
        {
            if (inst.IsPlaceholder)
                resolver.WriteBytesFor(inst.GetComponent(), response.OutputStream);
            else
            {
                var b = inst.GetVerbatimBytes();
                response.OutputStream.Write(b, 0, b.Length);
            }
        }
    }
}