namespace Ziewaar.Web.Uploadserver.Pages;

public class DynamicPageInstruction(FileText allText, int start, int length, bool isPlaceholder)
{
    private byte[]? BackingBinaryComponent;
    private string? BackingStringComponent;

    public readonly FileText AllText = allText;
    public readonly int Start = start, Length = length;
    public readonly bool IsPlaceholder = isPlaceholder;

    public string GetComponent()
    {
        BackingStringComponent ??= AllText.Text.Substring(Start, Length);
        return BackingStringComponent;
    }

    public byte[] GetVerbatimBytes()
    {
        if (BackingBinaryComponent != null) return BackingBinaryComponent;
        
        BackingBinaryComponent = new byte[Length * 4];
        var ct = Encoding.UTF8.GetBytes(AllText.Text, Start, Length, 
            BackingBinaryComponent, 0);
        Array.Resize(ref BackingBinaryComponent, ct);

        return BackingBinaryComponent;
    }
}