namespace Ziewaar.RAD.Doodads.Python;
public class PythonEval : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<PythonEnvironmentInteraction>(out var pei) || pei == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "python env required to run python"));
            return;
        }

        var tsi = new TextSinkingInteraction(interaction);
        OnElse?.Invoke(this, tsi);
        var scriptText = tsi.ReadAllText();

        var idd = new Dictionary<string, PyObject>(
            new InteractingDefaultingDictionary(interaction, EmptyReadOnlyDictionary.Instance).ToDictionary(x => x.Key,
                ConvertToPyObject));

        using var output = pei.Environment.ExecuteExpression(scriptText, idd);
        var result = output.ToString();
        
        OnThen?.Invoke(this, new CommonInteraction(interaction, result));
    }

    private PyObject ConvertToPyObject(KeyValuePair<string, object> x) =>
        PyObject.From(x.Value switch
        {
            decimal numval => (double)numval,
            { } otherVal => otherVal.ToString(),
        });

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}