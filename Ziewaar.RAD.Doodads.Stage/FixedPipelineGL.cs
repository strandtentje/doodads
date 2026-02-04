using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.Processing;

namespace Ziewaar.RAD.Doodads.Stage;
public sealed class FixedPipelineGL : IDisposable
{
    private readonly IVisualObserver VisualObserver;
    private readonly InterruptedVBO<TLQuad3> IncidentalQuad3Vbo, RegularQuad3Vbo;
    private readonly InterruptedVBO<TQuad2> InterfaceQuad2Vbo;
    public FixedPipelineGL(
        IVisualObserver visualObserver,
        int incidentalSize = 4096,
        int regularSize = 4096,
        int interfaceSize = 4096,
        int textureSize = 512)
    {
        this.VisualObserver = visualObserver;

        GL.Enable(EnableCap.Texture2D);
        GL.Enable(EnableCap.Light0);
        GL.Enable(EnableCap.ColorMaterial);
        GL.Enable(EnableCap.Normalize);

        GL.EnableClientState(ArrayCap.VertexArray);
        GL.EnableClientState(ArrayCap.ColorArray);
        GL.EnableClientState(ArrayCap.TextureCoordArray);
        GL.EnableClientState(ArrayCap.NormalArray);

        GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
        GL.ClearColor(0f, 0f, 0f, 1f);

        this.IncidentalQuad3Vbo = new(incidentalSize, textureSize);
        this.RegularQuad3Vbo = new(regularSize, textureSize);
        this.InterfaceQuad2Vbo = new(interfaceSize, textureSize);
    }
    public void Render()
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        this.VisualObserver.Accomodate3D();

        IncidentalQuad3Vbo.Scope().Draw();
        RegularQuad3Vbo.Scope().Draw();

        this.VisualObserver.Accomodate2D();
        
        InterfaceQuad2Vbo.Scope().Draw();
        
    }
    
    public void Dispose()
    {
        this.IncidentalQuad3Vbo.Dispose();
        this.RegularQuad3Vbo.Dispose();
        this.InterfaceQuad2Vbo.Dispose();
    }
}