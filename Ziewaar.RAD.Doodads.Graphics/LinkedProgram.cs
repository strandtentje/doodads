using OpenTK.Graphics.OpenGL;

namespace Ziewaar.RAD.Doodads.Graphics;
public class LinkedProgram(int programHandle) : IDisposable
{
    public static LinkedProgram FromShaders(params CompiledShader[] shaders)
    {
        var handle = GL.CreateProgram();
        try
        {
            foreach (var s in shaders) GL.AttachShader(handle, s.ShaderId);
            GL.LinkProgram(handle);
            GL.GetProgrami(handle, ProgramProperty.LinkStatus, out var code);
            if (code != (int)All.True)
                throw new ProgramLinkingException();
        }
        catch (Exception ex1)
        {
            try
            {
                foreach (var s in shaders) GL.DetachShader(handle, s.ShaderId);
            }
            catch (Exception ex2)
            {
                throw new AggregateException(ex1, ex2);
            }
            try
            {
                GL.DeleteProgram(handle);
            }
            catch (Exception ex3)
            {
                throw new AggregateException(ex1, ex3);
            }
            throw;
        }

        return new LinkedProgram(handle);
    }
    public int Handle => programHandle;
    public void Dispose() => GL.DeleteProgram(programHandle);
}