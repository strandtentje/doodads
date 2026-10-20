namespace Ziewaar.Web.Uploadserver.Buffers;

public class FixedBuffer
{
    public static readonly FixedBuffer Null = new FixedBuffer(new byte[0], 0);

    public readonly byte[] Allocation;
    public readonly int Count;

    public FixedBuffer(byte[] allocation, int count)
    {
        this.Allocation = allocation;
        this.Count = count;
    }
}