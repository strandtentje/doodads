using OpenTK.Mathematics;

namespace Ziewaar.RAD.Doodads.Stage;
public interface IVisualObserver
{
    int Width { get; }
    int Height { get; }
    
    /// <summary>
    /// go for ~70
    /// </summary>
    float Fov { get; }
    /// <summary>
    /// go for ~0.1
    /// </summary>
    float Near { get; }
    /// <summary>
    /// go for ~100
    /// </summary>
    float Far { get; }

    Vector3 CameraPosition { get; }
    Vector3 LookingAt { get; }
    Vector3 Up { get; }
}