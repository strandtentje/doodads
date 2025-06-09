namespace Ziewaar.RAD.Doodads.ModuleLoader.Interactions;
public class CallingInteraction(IInteraction offset, IReadOnlyDictionary<string, object> extra) : IInteraction
{
    public event EventHandler<IInteraction> OnThen, OnElse;
    public void InvokeOnThen(IInteraction interaction) => OnThen?.Invoke(this, interaction);
    public void InvokeOnElse(IInteraction interaction) => OnElse?.Invoke(this, interaction);
    public IInteraction Stack => offset;
    public object Register => offset.Register;
    public IReadOnlyDictionary<string, object> Memory => extra;
}
