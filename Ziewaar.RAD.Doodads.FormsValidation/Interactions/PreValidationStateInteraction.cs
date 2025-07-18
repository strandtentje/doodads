namespace Ziewaar.RAD.Doodads.FormsValidation.Interactions;
#pragma warning disable 67
public class PreValidationStateInteraction(string formName, TextSinkingInteraction parent) : IInteraction
{
    public string FormName => formName;
    public TextSinkingInteraction FieldSink => parent;
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public IInteraction? ProceedAt = null;
    public SortedList<string, bool> FieldValidations = new SortedList<string, bool>();
    public SortedList<string, object> FieldValues = new();
}