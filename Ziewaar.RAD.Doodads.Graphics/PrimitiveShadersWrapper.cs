using OpenTK.Graphics.OpenGL;

namespace Ziewaar.RAD.Doodads.Graphics;
public class PrimitiveShadersWrapper(LinkedProgram linkedProgram, Dictionary<string, int> dictionary)
{
    public static PrimitiveShadersWrapper FromFiles(string vertexShaderFile, string fragmentShaderFile)
    {
        using var vsf = CompiledShader.FromFile(vertexShaderFile, ShaderType.VertexShader);
        using var fsf = CompiledShader.FromFile(fragmentShaderFile, ShaderType.FragmentShader);
        var pgm = LinkedProgram.FromShaders(vsf, fsf);
        GL.GetProgrami(pgm.Handle, ProgramProperty.ActiveUniforms, out var numberOfUniforms);
        var uniformOffsets = new Dictionary<string, int>();
        for (uint i = 0; i < numberOfUniforms; i++)
        {
            var key = GL.GetActiveUniformName(pgm.Handle, i, 256, out var _);
            var location = GL.GetUniformLocation(pgm.Handle, key);
            uniformOffsets.Add(key, location);
        }
        return new PrimitiveShadersWrapper(pgm, uniformOffsets);
    }
    public int Handle => linkedProgram.Handle;
    public IReadOnlyDictionary<string, int> UniformLocations => dictionary;
}