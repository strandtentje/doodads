using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Ziewaar.RAD.Doodads.Stage;
public class FixedPipelineGLWindow() : GameWindow(new GameWindowSettings()
{
    UpdateFrequency = 100,
}, new NativeWindowSettings()
{
    SrgbCapable = false,
    Profile = ContextProfile.Compatability,
    Flags = ContextFlags.Default,
})
{
    private FixedPipelineGL FPL;
    protected override void OnLoad()
    {
        Console.WriteLine(GL.GetString(StringName.Version));
        Console.WriteLine(GL.GetString(StringName.Renderer));

        this.FPL = new FixedPipelineGL();
        this.FPL.LoadTexture("sample.png");
        this.FPL.SetProjection(this.ClientSize.X, this.ClientSize.Y);
    }
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        this.FPL.SetProjection(this.ClientSize.X, this.ClientSize.Y);
    }
    private Stopwatch sw = Stopwatch.StartNew();
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        this.FPL.BeginFrame();
        
        
        float[] lightDir = { 1f, 1f, 1f, 0f }; // w=0 → directional
        float[] white    = { 0.9f, 0.6f, 0.9f, 1f };
        float[] ambient  = { 0.1f, 0.4f, 0.1f, 1f };

        GL.Light(LightName.Light0, LightParameter.Position, lightDir);
        GL.Light(LightName.Light0, LightParameter.Diffuse, white);
        GL.Light(LightName.Light0, LightParameter.Specular, white);
        GL.Light(LightName.Light0, LightParameter.Ambient, ambient);
        
        this.FPL.SetCamera(new Vector3(0, 0, 5), Vector3.Zero);

        Matrix4 modeltransform = Matrix4.CreateRotationY(sw.ElapsedMilliseconds / 2000f) *
                                 Matrix4.CreateRotationX(sw.ElapsedMilliseconds / 3000f) *
                                 Matrix4.CreateRotationZ(sw.ElapsedMilliseconds / 5000f) *
                                 Matrix4.CreateTranslation(0, 0, 0);
        
        float[] matDiffuse  = { 1f, 1f, 1f, 1f };
        float[] matAmbient  = { 1f, 1f, 1f, 1f };
        float[] matSpecular = { 0.5f, 0.5f, 0.5f, 1f };

        GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, matDiffuse);
        GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, matAmbient);
        GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, matSpecular);
        GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, 32f);

        this.FPL.DrawQuad(modeltransform);

        SwapBuffers();
    }
    protected override void OnUnload()
    {
        base.OnUnload();
        this.FPL.Dispose();
    }
}