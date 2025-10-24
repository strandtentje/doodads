using OpenTK.Graphics.OpenGL;

namespace Ziewaar.RAD.Doodads.Graphics;
public class CompiledShader(int shaderId) : IDisposable
{
    public static CompiledShader FromFile(string path, ShaderType type)
    {
        var shader = GL.CreateShader(type);
        try
        {
            GL.ShaderSource(shader, File.ReadAllText(path));
            GL.CompileShader(shader);
            GL.GetShaderi(shader, ShaderParameterName.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                GL.GetShaderInfoLog(shader, out string infoLog);
                throw new ShaderCompilationException(
                    $"Error occurred while compiling Shader({shader}) from `{path}`.\n\n{infoLog}");
            }
        }
        catch
        {
            try
            {
                GL.DeleteShader(shader);
            }
            catch
            {
            }
            throw;
        }
        return new CompiledShader(shader);
    }
    public int ShaderId => shaderId;
    public void Dispose() => GL.DeleteShader(shaderId);
}