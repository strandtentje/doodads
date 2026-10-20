namespace Ziewaar.Web.Uploadserver.Models;

public enum ByteSequenceNodeTag
{
    StartingCrlf,
    StartingDashDash, 
    Boundary,
    DashDashBoundaryDashDash,
    DashDashBoundaryCrLf,
    FirstHeaderCrlf,
    SecondHeaderCrlf,
}