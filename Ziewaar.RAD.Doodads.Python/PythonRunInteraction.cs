namespace Ziewaar.RAD.Doodads.Python;
public class PythonRunInteraction(IInteraction stack, PyObject obj) : IInteraction
{
    public IInteraction Stack => stack;
    public object Register => stack.Register;
    public IReadOnlyDictionary<string, object> Memory => stack.Memory;
}