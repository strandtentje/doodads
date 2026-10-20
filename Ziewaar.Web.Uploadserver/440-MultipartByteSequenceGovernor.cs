namespace Ziewaar.Web.Uploadserver;

public class MultipartByteSequenceGovernor
{
    public readonly ByteSequenceNode
        BoundaryTree, DashDashBoundaryTree, CrlfDashDashBoundaryTree;
    public ByteSequenceNode Current { get; private set; }
    public MultipartByteSequenceGovernor()
    {
        BoundaryTree = ByteSequenceNode.CreateBoundaryTree();
        DashDashBoundaryTree = ByteSequenceNode.WrapDashDash(BoundaryTree);
        CrlfDashDashBoundaryTree = ByteSequenceNode.WrapCrlf(DashDashBoundaryTree);
        Current = DashDashBoundaryTree;
    }
    public void Rescope(byte[] newBoundary)
    {
        BoundaryTree.Sequence = newBoundary;
    }
    public void WithCrlf()
    {
        Current = CrlfDashDashBoundaryTree;
    }

    public void WithoutCrlf()
    {
        Current = DashDashBoundaryTree;
    }
}