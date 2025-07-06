namespace Ziewaar.RAD.Doodads.FormsValidation.Interactions;
#pragma warning disable 67
public class PostValidationStateInteraction(
    string fieldsInto,
    IInteraction canonicalInteraction,
    ISinkingInteraction? sinkingInteraction,
    SortedList<string, object> validationStateFieldValues,
    TextSinkingInteraction validationStateFieldSink) : IInteraction
{
    public IInteraction Stack { get; } = canonicalInteraction.ResurfaceToSink(sinkingInteraction);
    public object Register => canonicalInteraction.Register;
    public IReadOnlyDictionary<string, object> Memory => new FallbackReadOnlyDictionary(
        validationStateFieldValues,
        new SwitchingDictionary([fieldsInto], x => x == fieldsInto ? GetFields() : throw new KeyNotFoundException()));
    private string? FieldsBuffer = null;
    public string GetFields() => FieldsBuffer ??= validationStateFieldSink.ReadAllText();
}