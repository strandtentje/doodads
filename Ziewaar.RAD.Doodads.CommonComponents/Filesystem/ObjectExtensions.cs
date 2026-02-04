#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

public static class ObjectExtensions
{
    extension(object obj)
    {
        public bool IsntJustAnObject() => !obj.GetType().IsAssignableFrom(typeof(object));
    }
}
