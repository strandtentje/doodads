using System.Runtime.InteropServices;

namespace Ziewaar.RAD.Doodads.Stage;
[StructLayout(LayoutKind.Sequential)]
public struct TQuad2
{
    public TVertex2 P, Q, R, S; // 4 * 48 bytes = 192 bytes
}