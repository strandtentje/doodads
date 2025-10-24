using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Graphics;
public class GraphicsViewInteraction(IInteraction interaction, GraphicsViewWindow currentWindow) : IInteraction
{
    public GraphicsViewWindow Window => currentWindow;
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
}