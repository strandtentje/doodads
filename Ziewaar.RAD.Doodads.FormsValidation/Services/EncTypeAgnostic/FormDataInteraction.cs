namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;
public class FormDataInteraction(
    IInteraction stack,
    IEnumerable<IGrouping<string, object>> data) : IInteraction
{
    public IInteraction Stack => stack;
    public object Register => stack.Register;
    public IReadOnlyDictionary<string, object> Memory => stack.Memory;
    public IEnumerable<IGrouping<string, object>> Data => data;
}