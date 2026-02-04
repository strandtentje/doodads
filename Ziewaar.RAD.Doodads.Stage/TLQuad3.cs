using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace Ziewaar.RAD.Doodads.Stage;
[StructLayout(LayoutKind.Sequential)]
public struct TLQuad3 : IVboAligned
{
    public TLVertex3 P, Q, R, S; // 4 * 48 bytes = 192 bytes
    public static void Accomodate()
    {
        GL.VertexPointer(3, VertexPointerType.Float, TLVertex3.STRIDE, TLVertex3.OFFSET_POS);
        GL.ColorPointer(4, ColorPointerType.Float, TLVertex3.STRIDE, TLVertex3.OFFSET_RGBA);
        GL.TexCoordPointer(2, TexCoordPointerType.Float, TLVertex3.STRIDE, TLVertex3.OFFSET_UV);
        GL.NormalPointer(NormalPointerType.Float, TLVertex3.STRIDE, TLVertex3.OFFSET_NORMAL);
    }
}