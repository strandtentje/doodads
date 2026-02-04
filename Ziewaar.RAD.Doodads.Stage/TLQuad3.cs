using System.Runtime.InteropServices;

namespace Ziewaar.RAD.Doodads.Stage;
[StructLayout(LayoutKind.Sequential)]
public struct TLQuad3
{
    public TLVertex3 P, Q, R, S; // 4 * 48 bytes = 192 bytes
}