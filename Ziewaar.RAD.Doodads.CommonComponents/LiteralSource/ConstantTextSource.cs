namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

public class ConstantTextSource : IService
{
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        var writer = interaction.ResurfaceWriter();
        if (writer.RequireUpdate(serviceConstants.LastChange.Ticks))
            writer.TaggedData.Data.Write(serviceConstants.InsertIgnore("text", ""));
    }
}