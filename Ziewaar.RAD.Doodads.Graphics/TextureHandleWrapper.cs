using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.Vulkan;
using StbImageSharp;

namespace Ziewaar.RAD.Doodads.Graphics;
public class TextureHandleWrapper(int glHandle, TextureUnit unit)
{
    public static TextureHandleWrapper FromFile(string path, TextureUnit unit = TextureUnit.Texture0)
    {
        int handle = GL.GenTexture();
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2d, handle);
        StbImage.stbi_set_flip_vertically_on_load(1);
        using (Stream stream = File.OpenRead(path))
        {
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, image.Width, image.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        }
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.GenerateMipmap(TextureTarget.Texture2d);

        return new TextureHandleWrapper(handle, unit);
    }
    public void Use()
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2d, glHandle);
    }
}