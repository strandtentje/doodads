using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Ziewaar.RAD.Doodads.Stage;
public sealed class FixedPipelineGL : IDisposable
{
    private int TLQuad3VboId;
    private int TextureId;
    private TLQuad3[] IncidentalTLQuad3Memory = new TLQuad3[4096];
    private TLQuad3[] RegularTLQuad3Memory = new TLQuad3[4096];
    private TQuad2[] InterfaceTQuad2Memory = new TQuad2[512];
    
    private IntPtr TextureMemory;
    public FixedPipelineGL()
    {
        InitGL();
        CreateQuadVbo();
    }
    void InitGL()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Texture2D);
        GL.Enable(EnableCap.Lighting);
        GL.Enable(EnableCap.Light0);
        GL.Enable(EnableCap.ColorMaterial);
        GL.Enable(EnableCap.Normalize);

        GL.EnableClientState(ArrayCap.VertexArray);
        GL.EnableClientState(ArrayCap.ColorArray);
        GL.EnableClientState(ArrayCap.TextureCoordArray);
        GL.EnableClientState(ArrayCap.NormalArray);

        GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
        GL.ClearColor(0f, 0f, 0f, 1f);
    }
    void CreateQuadVbo()
    {
        IncidentalTLQuad3Memory[0] = new TLQuad3()
        {
            P = new TLVertex3()
            {
                Position = new Vector3(-1, -1, 0),
                Rgba = Color4.White,
                Uv = new Vector2(0, 0),
                Normal = new(0, 0, 1)
            },
            Q = new TLVertex3()
            {
                Position = new Vector3(1, -1, 0),
                Rgba = Color4.White,
                Uv = new Vector2(1, 0),
                Normal = new(0, 0, 1),
            },
            R = new TLVertex3()
            {
                Position = new Vector3(1, 1, 0),
                Rgba = Color4.White,
                Uv = new Vector2(1, 1),
                Normal = new(0, 0, 1),
            },
            S = new TLVertex3()
            {
                Position = new Vector3(-1, 1, 0),
                Rgba = Color4.White,
                Uv = new Vector2(0, 1),
                Normal = new(0, 0, 1),
            }
        };

        TLQuad3VboId = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, TLQuad3VboId);
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            IncidentalTLQuad3Memory.Length * Marshal.SizeOf<TLQuad3>(),
            IncidentalTLQuad3Memory,
            BufferUsageHint.DynamicDraw);
    }
    public void LoadTexture(string path)
    {
        TextureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, TextureId);

        using Image<Rgba32> image = Image<Rgba32>.Load<Rgba32>(path);
        image.Mutate(x => x.Flip(FlipMode.Vertical));
        byte[] intermediate = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(intermediate);
        this.TextureMemory = Marshal.AllocHGlobal(intermediate.Length);
        Marshal.Copy(intermediate, 0, TextureMemory, intermediate.Length);

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            image.Width,
            image.Height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            (IntPtr)TextureMemory);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
    }
    public void SetCamera(Vector3 eye, Vector3 target)
    {
        Matrix4 view = Matrix4.LookAt(eye, target, Vector3.UnitY);

        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadMatrix(ref view);
    }
    public void SetProjection(float vpw, float vph, float fovDegrees = 70f, float near = 0.1f, float far = 100f)
    {
        GL.Viewport(0, 0, (int)vpw, (int)vph);

        Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(fovDegrees),
            vpw / vph,
            near,
            far);

        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadMatrix(ref proj);

        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadIdentity();
    }
    public void BeginFrame()
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }
    public void DrawQuad(Matrix4 model)
    {
        GL.BindTexture(TextureTarget.Texture2D, TextureId);
        GL.BindBuffer(BufferTarget.ArrayBuffer, TLQuad3VboId);

        GL.VertexPointer(3, VertexPointerType.Float, TLVertex3.STRIDE, TLVertex3.OFFSET_POS);
        GL.ColorPointer(4, ColorPointerType.Float, TLVertex3.STRIDE, TLVertex3.OFFSET_RGBA);
        GL.TexCoordPointer(2, TexCoordPointerType.Float, TLVertex3.STRIDE, TLVertex3.OFFSET_UV);
        GL.NormalPointer(NormalPointerType.Float, TLVertex3.STRIDE, TLVertex3.OFFSET_NORMAL);

        GL.PushMatrix();
        GL.MultMatrix(ref model);

        GL.DrawArrays(PrimitiveType.Quads, 0, 4);

        GL.PopMatrix();
    }
    public void Dispose()
    {
        if (TLQuad3VboId != 0)
            GL.DeleteBuffer(TLQuad3VboId);

        if (TextureId != 0)
            GL.DeleteTexture(TextureId);

        if (this.TextureMemory != IntPtr.Zero)
            Marshal.FreeHGlobal(TextureMemory);
    }
}