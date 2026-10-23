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
    public void Render(IHttpLifecycleWrapper lifecycle,
        IPlaceholderResolver resolver)
    {
        long contentLength = 0;
        lifecycle.ResponseEncoding ??= Encoding.UTF8;
        foreach (var inst in Instructions)
        {
            if (inst.IsPlaceholder)
                contentLength += resolver.GetByteLengthFor(lifecycle.ResponseEncoding, inst.GetComponent());
            else
                contentLength += inst.GetVerbatimBytes().Length;
        }

        lifecycle.TrySetResponseHeader(contentLength, "text/html");

        foreach (var inst in Instructions)
        {
            if (inst.IsPlaceholder)
                resolver.WriteBytesFor(Encoding.UTF8, inst.GetComponent(), lifecycle.ResponseStream);
            else
            {
                var b = inst.GetVerbatimBytes();
                lifecycle.ResponseStream.Write(b, 0, b.Length);
            }
        }
    }
}