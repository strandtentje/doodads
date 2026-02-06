using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Stage;
public class GlWindowInteraction(IInteraction interaction, FixedPipelineGLWindow wnd) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public FixedPipelineGLWindow Window => wnd;
}