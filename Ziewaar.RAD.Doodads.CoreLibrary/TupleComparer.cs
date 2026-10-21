namespace Ziewaar.RAD.Doodads.CoreLibrary;

public class TupleComparer<T1, T2>(IComparer<T1> comparer1, IComparer<T2> comparer2)
    : IComparer<(T1 member1, T2 member2)>
{
    public int Compare((T1 member1, T2 member2) x, (T1 member1, T2 member2) y) =>
        comparer1.Compare(x.member1, y.member1) switch
        {
            < 0 => -1,
            > 0 => 1,
            _ => comparer2.Compare(x.member2, y.member2) switch
            {
                < 0 => -1,
                > 0 => 1,
                _ => 0
            }
        };
}

public class TupleComparer
{
    public static TupleComparer<string, string> OrdinalIgnoreCase => field ??= new TupleComparer<string, string>(
        StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
}