using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.Python;

[Category("Interop & Languages")]
[Title("Evaluate python expression")]
[Description("""
             Provided a python environment, evaluates a python expression.
             """)]
public class PythonEval : IService
{
    [EventOccasion("Will contain output of python expression in register here.")]
    public event CallForInteraction? OnThen;

    [EventOccasion("Sink python expression text here.")]
    public event CallForInteraction? OnElse;

    [EventOccasion("Likely happens when no python environment was setup.")]
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
            new InteractionMirroringDictionary(interaction).ToDictionary(x => x.Key,
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