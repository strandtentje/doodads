namespace Ziewaar.RAD.Doodads.Stage;
public class Continuity(int capacity)
{
    public uint Start, End;
    public Continuity? Next = null;
    public void Include(int index)
    {
        throw new NotImplementedException();
    }
}