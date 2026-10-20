namespace Ziewaar.Web.Uploadserver.Pages;

public class DynamicPageInstruction
{
    private byte[] BackingBinaryComponent;
    private string BackingStringComponent;

    public readonly FileText AllText;
    public readonly int Start, Length;
    public readonly bool IsPlaceholder;
    public DynamicPageInstruction(FileText allText, 
        int start, int length, bool isPlaceholder)
    {
        this.AllText = allText;
        this.Start = start;
        this.Length = length;
        this.IsPlaceholder = isPlaceholder;
    }

    public string GetComponent()
    {
        if (BackingStringComponent == null)
            BackingStringComponent = AllText.Text.Substring(Start, Length);
        return BackingStringComponent;
    }

    public byte[] GetVerbatimBytes()
    {
        if (BackingBinaryComponent == null)
        {
            BackingBinaryComponent = new byte[Length * 4];
            var ct = Encoding.UTF8.GetBytes(AllText.Text, Start, Length, 
                BackingBinaryComponent, 0);
            Array.Resize(ref BackingBinaryComponent, ct);
        }
             
        return BackingBinaryComponent;
    }
}