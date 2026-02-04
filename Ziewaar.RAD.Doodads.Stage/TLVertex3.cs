using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace Ziewaar.RAD.Doodads.Stage;
[StructLayout(LayoutKind.Sequential)]
public struct TLVertex3
{
    public Vector3 Position; // 4 * 3 = 12 bytes
    public Color4 Rgba; // 4 * 4 = 16 bytes
    public Vector2 Uv; // 4 * 2 = 8 bytes
    public Vector3 Normal; // 4 * 3 = 12 bytes
    public const int
        OFFSET_POS = 0,
        OFFSET_RGBA = OFFSET_POS + sizeof(float) * 3,
        OFFSET_UV = OFFSET_RGBA + sizeof(float) * 4,
        OFFSET_NORMAL = OFFSET_UV + sizeof(float) * 2,
        STRIDE = OFFSET_NORMAL + sizeof(float) * 3;
}