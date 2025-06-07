
namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

public class ConstantTextSource : IService
{
    public event EventHandler<IInteraction> OnError;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        if (!interaction.TryRequireStreamingUpdate(serviceConstants.LastChange.Ticks, 
            out var source, out var writer, out var delimiter))
            return;
        if (source.IsContentTypeApplicable(serviceConstants.InsertIgnore("content-type", "*/*")))
        {
            writer.Write(serviceConstants.InsertIgnore("text", ""));
            writer.Write(delimiter);
            writer.Flush();
        } else
        {
            OnError?.Invoke(this, 
                VariablesInteraction.ForError(interaction, "Update required but no content type applicable"));
        }
    }
}

