using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Ziewaar.RAD.Doodads.Stage;
public static class VisualObserverExtensions
{
    public static float GetWidthDominantAspect(this IVisualObserver visualObserver) =>
        ((float)visualObserver.Width) / ((float)visualObserver.Height);
    public static float GetHeightDominantAspect(this IVisualObserver visualObserver) =>
        ((float)visualObserver.Height) / ((float)visualObserver.Width);
    public static void Accomodate3D(this IVisualObserver visualObserver)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Lighting);
        GL.Enable(EnableCap.Texture2D);

        GL.Viewport(0, 0, visualObserver.Width, visualObserver.Height);
        Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(visualObserver.Fov),
            visualObserver.GetWidthDominantAspect(),
            visualObserver.Near,
            visualObserver.Far);

        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadMatrix(ref proj);

        GL.MatrixMode(MatrixMode.Modelview);
        Matrix4 view = Matrix4.LookAt(
            visualObserver.CameraPosition,
            visualObserver.LookingAt,
            visualObserver.Up);

        GL.LoadIdentity();
        GL.LoadMatrix(ref view);
    }
    public static void Accomodate2D(this IVisualObserver visualObserver)
    {
        GL.Disable(EnableCap.Lighting);
        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();

        var width = Math.Max(1, visualObserver.GetWidthDominantAspect());
        var height = Math.Max(1, visualObserver.GetHeightDominantAspect());

        GL.Ortho(0, width, height, 0, -1, 1);

        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadIdentity();
    }
}