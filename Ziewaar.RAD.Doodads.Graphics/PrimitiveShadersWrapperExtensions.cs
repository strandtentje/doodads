using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Ziewaar.RAD.Doodads.Graphics;
public static class PrimitiveShadersWrapperExtensions
{
    public static void Use(this PrimitiveShadersWrapper wrapper) => GL.UseProgram(wrapper.Handle);
    public static bool TryUseUniformByName(this PrimitiveShadersWrapper wrapper, string name, out int location)
    {
        if (!wrapper.UniformLocations.TryGetValue(name, out location))
            return false;
        wrapper.Use();
        return true;
    }
    public static int GetAttribLocation(this PrimitiveShadersWrapper wrapper, string attribName) =>
        GL.GetAttribLocation(wrapper.Handle, attribName);
    public static void Set(this PrimitiveShadersWrapper wrapper, string name, int data)
    {
        if (!wrapper.TryUseUniformByName(name, out var location))
            throw new KeyNotFoundException();
        else
            GL.Uniform1i(location, data);
    }
    public static void Set(this PrimitiveShadersWrapper wrapper, string name, float data)
    {
        if (!wrapper.TryUseUniformByName(name, out var location))
            throw new KeyNotFoundException();
        else
            GL.Uniform1f(location, data);
    }
    public static void Set(this PrimitiveShadersWrapper wrapper, string name, Matrix4 data)
    {
        if (!wrapper.TryUseUniformByName(name, out var location))
            throw new KeyNotFoundException();
        else
            GL.UniformMatrix4f(location, 1, true, ref data);
    }
    public static void SetVector3(this PrimitiveShadersWrapper wrapper, string name, Vector3 data)
    {
        if (!wrapper.TryUseUniformByName(name, out var location))
            throw new KeyNotFoundException();
        else
            GL.Uniform3f(location, data.X, data.Y, data.Z);
    }
}