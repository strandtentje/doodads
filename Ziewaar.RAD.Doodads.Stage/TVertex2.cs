using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace Ziewaar.RAD.Doodads.Stage;
[StructLayout(LayoutKind.Sequential)]
public struct TVertex2
{
    public Vector2 Position; // 4 * 3 = 12 bytes
    public Color4 Rgba; // 4 * 4 = 16 bytes
    public Vector2 Uv; // 4 * 2 = 8 bytes
    public const int
        OFFSET_POS = 0,
        OFFSET_RGBA = OFFSET_POS + sizeof(float) * 3,
        OFFSET_UV = OFFSET_RGBA + sizeof(float) * 4,
        STRIDE = OFFSET_UV + sizeof(float) * 2;
}