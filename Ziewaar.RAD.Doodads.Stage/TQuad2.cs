using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Ziewaar.RAD.Doodads.Stage;
[StructLayout(LayoutKind.Sequential)]
public struct TQuad2 : IVboAligned, IQuiltSegmentAdaptor
{
    public TVertex2 P, Q, R, S;
    
    Vector2 IQuiltSegmentAdaptor.P { set => P.Uv = value; }
    Vector2 IQuiltSegmentAdaptor.Q { set => Q.Uv = value; }
    Vector2 IQuiltSegmentAdaptor.R { set => R.Uv = value; }
    Vector2 IQuiltSegmentAdaptor.S { set => S.Uv = value; }
    
    public static void Accomodate()
    {
        GL.VertexPointer(2, VertexPointerType.Float, TVertex2.STRIDE, TVertex2.OFFSET_POS);
        GL.ColorPointer(4, ColorPointerType.Float, TVertex2.STRIDE, TVertex2.OFFSET_RGBA);
        GL.TexCoordPointer(2, TexCoordPointerType.Float, TVertex2.STRIDE, TVertex2.OFFSET_UV);
    }
}